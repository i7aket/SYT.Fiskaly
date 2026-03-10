using System.Reflection;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SYT.Fiskaly;
using SYT.Fiskaly.Authentication;
using SYT.Fiskaly.Configuration;
using SYT.Fiskaly.Management.ApiKeys;
using SYT.Fiskaly.Management.Organizations;
using SYT.Fiskaly.SignDE.Tss;

namespace SYT.Fiskaly.UnitTests.Configuration;

public class ServiceCollectionExtensionsTests
{
    private const string ValidApiSecret = "1234567890123456789012345678901234567890123";

    [Trait("Category", "Unit")]
    [Fact]
    public void AddFiskaly_ResolvesControlPlaneServices()
    {
        ServiceProvider serviceProvider = CreateServiceProvider();

        serviceProvider.GetRequiredService<IFiskalyCredentialScopeFactory>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IOrganizationClient>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IApiKeyClient>().Should().NotBeNull();
        serviceProvider.GetRequiredService<ITssClient>().Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddFiskaly_UsesManagementBaseUrlForManagementClients()
    {
        ServiceProvider serviceProvider = CreateServiceProvider();

        IOrganizationClient organizationClient = serviceProvider.GetRequiredService<IOrganizationClient>();
        IApiKeyClient apiKeyClient = serviceProvider.GetRequiredService<IApiKeyClient>();

        HttpClient organizationHttpClient = GetHttpClient(organizationClient);
        HttpClient apiKeyHttpClient = GetHttpClient(apiKeyClient);

        organizationHttpClient.BaseAddress!.ToString().Should().Be("https://dashboard.fiskaly.com/api/v0/");
        apiKeyHttpClient.BaseAddress!.ToString().Should().Be("https://dashboard.fiskaly.com/api/v0/");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddFiskaly_UsesMiddlewareBaseUrlForSignDeClients()
    {
        ServiceProvider serviceProvider = CreateServiceProvider();

        ITssClient tssClient = serviceProvider.GetRequiredService<ITssClient>();
        HttpClient tssHttpClient = GetHttpClient(tssClient);

        tssHttpClient.BaseAddress!.ToString().Should().Be("https://kassensichv-middleware.fiskaly.com/api/v2/");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddFiskaly_BindsIndependentClientConfigurations()
    {
        ServiceProvider serviceProvider = CreateServiceProvider();

        FiskalyConfiguration config = serviceProvider.GetRequiredService<IOptions<FiskalyConfiguration>>().Value;

        config.OrganizationClient.TimeoutSeconds.Should().Be(41);
        config.ApiKeyClient.TimeoutSeconds.Should().Be(43);
        config.TssClient.TimeoutSeconds.Should().Be(47);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        Dictionary<string, string?> settings = new()
        {
            ["Fiskaly:ApiKey"] = "test_key_for_service_collection_tests",
            ["Fiskaly:ApiSecret"] = ValidApiSecret,
            ["Fiskaly:BaseUrl"] = "https://kassensichv-middleware.fiskaly.com/api/v2/",
            ["Fiskaly:ManagementBaseUrl"] = "https://dashboard.fiskaly.com/api/v0/",
            ["Fiskaly:OrganizationClient:TimeoutSeconds"] = "41",
            ["Fiskaly:ApiKeyClient:TimeoutSeconds"] = "43",
            ["Fiskaly:TssClient:TimeoutSeconds"] = "47"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        ServiceCollection services = new();
        services.AddLogging();
        services.AddFiskaly(configuration);
        return services.BuildServiceProvider();
    }

    private static HttpClient GetHttpClient<TClient>(TClient client)
    {
        FieldInfo field = client!.GetType().GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Private _httpClient field was not found on {client.GetType().FullName}.");

        return (HttpClient)(field.GetValue(client)
            ?? throw new InvalidOperationException($"Private _httpClient field on {client.GetType().FullName} is null."));
    }
}
