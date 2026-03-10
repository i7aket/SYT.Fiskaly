using AwesomeAssertions;
using Microsoft.Extensions.Options;
using SYT.Fiskaly.Configuration;
using SYT.Fiskaly.Handlers;

namespace SYT.Fiskaly.UnitTests.Handlers;

public class FiskalyManagementBaseUrlHandlerTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public async Task SendAsync_WithRelativeUri_PrependsManagementBaseUrl()
    {
        RecordingHandler innerHandler = new();
        FiskalyManagementBaseUrlHandler handler = CreateHandler("https://dashboard.fiskaly.com/api/v0", innerHandler);
        HttpMessageInvoker invoker = new(handler);

        HttpResponseMessage response = await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "organizations?limit=10"),
            CancellationToken.None);

        response.RequestMessage!.RequestUri!.ToString().Should().Be("https://dashboard.fiskaly.com/api/v0/organizations?limit=10");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task SendAsync_WithAbsoluteUri_RewritesHostAndPreservesPathAndQuery()
    {
        RecordingHandler innerHandler = new();
        FiskalyManagementBaseUrlHandler handler = CreateHandler("https://dashboard.fiskaly.com/api/v0/", innerHandler);
        HttpMessageInvoker invoker = new(handler);

        HttpResponseMessage response = await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://example.org/organizations/123/api-keys?offset=20"),
            CancellationToken.None);

        response.RequestMessage!.RequestUri!.ToString().Should().Be("https://dashboard.fiskaly.com/organizations/123/api-keys?offset=20");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task SendAsync_WithNullRequestUri_UsesManagementBaseUrlAsRequestUri()
    {
        RecordingHandler innerHandler = new();
        FiskalyManagementBaseUrlHandler handler = CreateHandler("https://dashboard.fiskaly.com/api/v0/", innerHandler);
        HttpMessageInvoker invoker = new(handler);
        HttpRequestMessage request = new(HttpMethod.Get, requestUri: (Uri?)null);

        HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        response.RequestMessage!.RequestUri!.ToString().Should().Be("https://dashboard.fiskaly.com/api/v0/");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task SendAsync_WithInvalidManagementBaseUrl_LeavesRequestUriUnchanged()
    {
        RecordingHandler innerHandler = new();
        FiskalyManagementBaseUrlHandler handler = CreateHandler("not-a-valid-uri", innerHandler);
        HttpMessageInvoker invoker = new(handler);

        HttpResponseMessage response = await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "organizations"),
            CancellationToken.None);

        response.RequestMessage!.RequestUri!.OriginalString.Should().Be("organizations");
    }

    private static FiskalyManagementBaseUrlHandler CreateHandler(string managementBaseUrl, HttpMessageHandler innerHandler)
    {
        TestOptionsMonitor options = new(new FiskalyConfiguration
        {
            ManagementBaseUrl = managementBaseUrl
        });

        return new FiskalyManagementBaseUrlHandler(options)
        {
            InnerHandler = innerHandler
        };
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                RequestMessage = request
            });
        }
    }

    private sealed class TestOptionsMonitor(FiskalyConfiguration currentValue) : IOptionsMonitor<FiskalyConfiguration>
    {
        public FiskalyConfiguration CurrentValue { get; } = currentValue;

        public FiskalyConfiguration Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<FiskalyConfiguration, string?> listener) => null;
    }
}
