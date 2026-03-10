using System.Diagnostics.Metrics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using SYT.Fiskaly.Authentication;
using SYT.Fiskaly.Configuration;
using SYT.Fiskaly.Exceptions;
using SYT.Fiskaly.Handlers;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.Metrics;
using SYT.Fiskaly.Resilience;
using SYT.Fiskaly.Serialization;
using SYT.Fiskaly.SignDE.Admin;
using SYT.Fiskaly.SignDE.Clients;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Exports;
using SYT.Fiskaly.SignDE.Exports.Dsfinvk;
using SYT.Fiskaly.SignDE.Transactions;
using SYT.Fiskaly.SignDE.Transactions.Serialization;
using SYT.Fiskaly.SignDE.Tss;
using SYT.Fiskaly.Management.ApiKeys;
using SYT.Fiskaly.Management.Organizations;

namespace SYT.Fiskaly;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFiskaly(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "Fiskaly",
        Action<FiskalyConfiguration>? configure = null)
    {
        #region Configuration & Validation

        OptionsBuilder<FiskalyConfiguration> optionsBuilder = services.AddOptions<FiskalyConfiguration>();

        IConfigurationSection fiskalySection = configuration.GetSection(configSectionName);
        if (fiskalySection.Exists())
        {
            optionsBuilder.Bind(fiskalySection);
        }

        if (configure != null)
        {
            optionsBuilder.Configure(configure);
        }

        services.AddSingleton<IValidateOptions<FiskalyConfiguration>, FiskalyConfigurationValidator>();
        optionsBuilder.ValidateOnStart();

        #endregion Configuration & Validation

        #region Core Infrastructure

        services.TryAddSingleton<IMeterFactory, DefaultMeterFactory>();

        services.TryAddSingleton(TimeProvider.System);

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // For robust deserialization
            Converters =
            {
                new MoneyAmountJsonConverter(),
                new NullableLongJsonConverter(),
                new JsonStringEnumConverter(),
                new MetadataCollectionJsonConverter(),
                new UnixEpochDateTimeOffsetConverterFactory()
            }
        };

        services.AddSingleton(jsonOptions);

        services.AddSingleton<IDsfinvkVersionStrategy, DsfinvkV2SegmentStrategy>();

        services.AddSingleton<FiskalyMetrics>();

        #endregion Core Infrastructure

        #region HTTP Infrastructure

        services.AddScoped<FiskalyHttpRequestExecutor>(sp =>
            new FiskalyHttpRequestExecutor(
                sp.GetRequiredService<JsonSerializerOptions>(),
                sp.GetRequiredService<ILogger<FiskalyHttpRequestExecutor>>()));

        services.AddTransient<FiskalyBaseUrlHandler>();
        services.AddTransient<FiskalyManagementBaseUrlHandler>();

        services.AddHttpClient("FiskalyAuth", ConfigureFiskalyHttpClient)
            .AddHttpMessageHandler<FiskalyBaseUrlHandler>()
            .AddFiskalyResilience("FiskalyAuth", cfg => cfg.AuthClient)
            .AddHttpMessageHandler<FiskalyErrorHandler>();

        services.AddSingleton<IFiskalyAuthenticationService, FiskalyAuthenticationService>();
        services.AddSingleton<IFiskalyCredentialScopeFactory, FiskalyCredentialScopeFactory>();

        services.AddTransient<JwtAuthHandler>();

        services.AddTransient<FiskalyErrorHandler>();

        #endregion HTTP Infrastructure

        #region API Clients


        services.AddFiskalyTypedClient<IAdminClient, AdminClient>(
            "AdminClient", cfg => cfg.AdminClient);

        services.AddFiskalyNamedClient("TssClient", cfg => cfg.TssClient);
        services.AddFiskalyScopedClient<ITssClient, TssClient>("TssClient");

        services.AddFiskalyTypedClient<IClientManagementClient, ClientManagementClient>(
            "ClientManagementClient", cfg => cfg.ClientManagementClient);

        services.AddFiskalyNamedClient("TransactionClient", cfg => cfg.TransactionClient);
        services.AddFiskalyScopedClient<ITransactionClient, TransactionClient>("TransactionClient");

        services.AddFiskalyNamedClient("ExportClient", cfg => cfg.ExportClient);
        services.AddFiskalyScopedClient<IExportClient, ExportClient>(
            "ExportClient",
            sp => [sp.GetRequiredService<IDsfinvkVersionStrategy>()]);

        services.AddFiskalyManagementClient<IOrganizationClient, OrganizationClient>(
            "OrganizationClient", cfg => cfg.OrganizationClient);
        services.AddFiskalyManagementClient<IApiKeyClient, ApiKeyClient>(
            "ApiKeyClient", cfg => cfg.ApiKeyClient);

        #endregion API Clients

        return services;
    }

    #region Private Helper Methods

    private static void ConfigureFiskalyHttpClient(IServiceProvider sp, HttpClient client)
    {
        FiskalyConfiguration config = sp.GetRequiredService<IOptionsMonitor<FiskalyConfiguration>>().CurrentValue;
        client.BaseAddress = new Uri(config.BaseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;  // Polly handles timeouts
    }

    private static void ConfigureFiskalyManagementHttpClient(IServiceProvider sp, HttpClient client)
    {
        FiskalyConfiguration config = sp.GetRequiredService<IOptionsMonitor<FiskalyConfiguration>>().CurrentValue;
        client.BaseAddress = new Uri(config.ManagementBaseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;  // Polly handles timeouts
    }

    private static IHttpClientBuilder AddFiskalyPipeline(
        this IHttpClientBuilder builder,
        string clientName,
        Func<FiskalyConfiguration, FiskalyClientConfiguration> configSelector)
    {
        return builder
            .AddHttpMessageHandler<FiskalyBaseUrlHandler>()
            .AddHttpMessageHandler<JwtAuthHandler>()
            .AddFiskalyResilience(clientName, configSelector)
            .AddHttpMessageHandler<FiskalyErrorHandler>();
    }

    private static IHttpClientBuilder AddFiskalyManagementPipeline(
        this IHttpClientBuilder builder,
        string clientName,
        Func<FiskalyConfiguration, FiskalyClientConfiguration> configSelector)
    {
        return builder
            .AddHttpMessageHandler<JwtAuthHandler>()
            .AddFiskalyResilience(clientName, configSelector)
            .AddHttpMessageHandler<FiskalyErrorHandler>();
    }

    private static IHttpClientBuilder AddFiskalyTypedClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        Func<FiskalyConfiguration, FiskalyClientConfiguration> configSelector)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services
            .AddHttpClient<TClient, TImplementation>(ConfigureFiskalyHttpClient)
            .AddFiskalyPipeline(clientName, configSelector);
    }

    private static IHttpClientBuilder AddFiskalyNamedClient(
        this IServiceCollection services,
        string clientName,
        Func<FiskalyConfiguration, FiskalyClientConfiguration> configSelector)
    {
        return services
            .AddHttpClient(clientName, ConfigureFiskalyHttpClient)
            .AddFiskalyPipeline(clientName, configSelector);
    }

    private static IHttpClientBuilder AddFiskalyManagementClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        Func<FiskalyConfiguration, FiskalyClientConfiguration> configSelector)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services
            .AddHttpClient<TClient, TImplementation>(ConfigureFiskalyManagementHttpClient)
            .AddFiskalyManagementPipeline(clientName, configSelector);
    }

    private static void AddFiskalyScopedClient<TInterface, TImplementation>(
        this IServiceCollection services,
        string httpClientName,
        Func<IServiceProvider, object[]>? additionalDependencies = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TInterface>(sp =>
        {
            object[] standardArgs = new object[]
            {
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName),
                sp.GetRequiredService<FiskalyHttpRequestExecutor>(),
                sp.GetRequiredService<ILogger<TImplementation>>(),
                sp.GetRequiredService<JsonSerializerOptions>()
            };

            object[] extraArgs = additionalDependencies?.Invoke(sp) ?? [];

            return ActivatorUtilities.CreateInstance<TImplementation>(
                sp,
                [..standardArgs, ..extraArgs]);
        });
    }

    private static IHttpClientBuilder AddFiskalyResilience(
        this IHttpClientBuilder builder,
        string pipelineName,
        Func<FiskalyConfiguration, FiskalyClientConfiguration> selector)
    {
        IHttpStandardResiliencePipelineBuilder pipelineBuilder = builder.AddStandardResilienceHandler();

        pipelineBuilder.Configure((options, serviceProvider) =>
        {
            IOptionsMonitor<FiskalyConfiguration> monitor = serviceProvider.GetRequiredService<IOptionsMonitor<FiskalyConfiguration>>();
            ILoggerFactory? loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            ILogger? logger = loggerFactory?.CreateLogger("SYT.Fiskaly.HttpResilience");
            FiskalyMetrics metrics = serviceProvider.GetRequiredService<FiskalyMetrics>();

            FiskalyClientConfiguration GetClientConfig()
            {
                return selector(monitor.CurrentValue);
            }

            options.AttemptTimeout.TimeoutGenerator = _ =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();
                int timeoutSeconds = Math.Max(1, cfg.TimeoutSeconds);
                return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(timeoutSeconds));
            };

            options.TotalRequestTimeout.TimeoutGenerator = _ =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();
                int timeoutSeconds = Math.Max(1, cfg.TimeoutSeconds);
                int totalTimeoutFactor = cfg.ResilienceEnabled
                    ? Math.Max(1, cfg.RetryCount + 1)
                    : 1;
                return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds * totalTimeoutFactor)));
            };

            // Keep this valid for options validation; actual retries are gated in ShouldHandle.
            options.Retry.MaxRetryAttempts = 10;

            options.Retry.DelayGenerator = args =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();

                if (args.Outcome.Exception is FiskalyApiException fiskalyEx)
                {
                    if (fiskalyEx.RetryAfter.HasValue &&
                        fiskalyEx.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        return new ValueTask<TimeSpan?>(fiskalyEx.RetryAfter.Value);
                    }
                }

                TimeSpan delay = FiskalyDelayCalculator.CalculateDelay(
                    args.Outcome.Exception,
                    args.AttemptNumber,
                    cfg.CategoryDelays);

                return new ValueTask<TimeSpan?>(delay);
            };

            options.Retry.ShouldHandle = args =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();

                if (!cfg.ResilienceEnabled)
                    return ValueTask.FromResult(false);

                if (args.AttemptNumber >= cfg.RetryCount)
                    return ValueTask.FromResult(false);

                bool shouldRetryException =
                    FiskalyResiliencePredicates.ShouldHandleTransient(args.Outcome) ||
                    FiskalyResiliencePredicates.ShouldHandleInfrastructure(args.Outcome) ||
                    FiskalyResiliencePredicates.ShouldHandleAuthentication(args.Outcome);

                if (shouldRetryException)
                    return ValueTask.FromResult(true);

                return ValueTask.FromResult(
                    FiskalyResiliencePredicates.ShouldHandleHttpTransient(args.Outcome));
            };

            options.Retry.OnRetry = args =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();

                logger?.LogWarning(
                    "Fiskaly HTTP request failed on pipeline {Pipeline} (attempt {Attempt}/{Max}). Retrying after {Delay}s.",
                    pipelineName,
                    args.AttemptNumber,
                    cfg.RetryCount,
                    args.RetryDelay.TotalSeconds);

                FiskalyApiException? exception = args.Outcome.Exception as FiskalyApiException;
                string category = exception?.Category.ToString() ?? "Unknown";

                metrics.ResilienceRetries.Add(1,
                    new KeyValuePair<string, object?>("resilience.pipeline", pipelineName),
                    new KeyValuePair<string, object?>("error.category", category));

                return ValueTask.CompletedTask;
            };

            options.CircuitBreaker.ShouldHandle = args =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();

                if (!cfg.ResilienceEnabled || cfg.CircuitBreakerThreshold <= 0)
                    return ValueTask.FromResult(false);

                bool shouldBreakOnException =
                    FiskalyResiliencePredicates.ShouldHandleInfrastructure(args.Outcome) ||
                    FiskalyResiliencePredicates.ShouldHandleHttpTransient(args.Outcome);

                if (shouldBreakOnException)
                    return ValueTask.FromResult(true);

                if (args.Outcome.Exception is FiskalyApiException apiException)
                {
                    int statusCode = (int)apiException.StatusCode;
                    return ValueTask.FromResult(statusCode >= 500 && statusCode <= 599);
                }

                return ValueTask.FromResult(false);
            };

            options.CircuitBreaker.OnOpened = _ =>
            {
                FiskalyClientConfiguration cfg = GetClientConfig();

                logger?.LogError(
                    "Fiskaly circuit breaker opened on pipeline {Pipeline} after {Failures} failures. Break duration: {Duration}s.",
                    pipelineName,
                    cfg.CircuitBreakerThreshold,
                    cfg.CircuitBreakerDurationSeconds);

                metrics.ResilienceCircuitBreakerOpened.Add(1,
                    new KeyValuePair<string, object?>("resilience.pipeline", pipelineName));

                return ValueTask.CompletedTask;
            };

            options.CircuitBreaker.OnClosed = _ =>
            {
                logger?.LogInformation("Fiskaly circuit breaker reset on pipeline {Pipeline}.", pipelineName);
                return ValueTask.CompletedTask;
            };

            FiskalyClientConfiguration initialConfig = GetClientConfig();
            TimeSpan initialAttemptTimeout = TimeSpan.FromSeconds(Math.Max(1, initialConfig.TimeoutSeconds));
            options.CircuitBreaker.MinimumThroughput = Math.Max(2, initialConfig.CircuitBreakerThreshold);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(Math.Max(60, initialAttemptTimeout.TotalSeconds * 2));
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(Math.Max(1, initialConfig.CircuitBreakerDurationSeconds));
        });

        return builder;
    }

    #endregion Private Helper Methods
}
