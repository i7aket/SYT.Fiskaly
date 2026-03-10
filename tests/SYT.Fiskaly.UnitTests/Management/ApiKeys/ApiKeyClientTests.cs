using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using SYT.Fiskaly.Management.ApiKeys;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Common.Enums;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.Management.ApiKeys.Models;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.Management.ApiKeys.Responses;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.UnitTests.Management.ApiKeys;

public class ApiKeyClientTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new MetadataCollectionJsonConverter()
        }
    };

    [Trait("Category", "Unit")]
    [Fact]
    public async Task CreateApiKeyAsync_PostsToOrganizationScopedEndpoint()
    {
        OrganizationId organizationId = OrganizationId.From("550e8400-e29b-41d4-a716-446655440000");
        string? requestBody = null;

        Mock<HttpMessageHandler> handler = new();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.RequestUri!.PathAndQuery == $"/api/v0/organizations/{organizationId}/api-keys"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "_id": "6f9619ff-8b86-4d01-b42d-00cf4fc964ff",
                          "name": "runtime-key",
                          "key": "test_runtime_key",
                          "secret": "1234567890123456789012345678901234567890123",
                          "status": "enabled"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            });

        ApiKeyClient sut = CreateClient(handler.Object);

        ApiKeyResponse response = await sut.CreateApiKeyAsync(
            organizationId,
            new CreateApiKeyRequest
            {
                Name = "runtime-key",
                Status = ApiKeyStatus.Enabled
            });

        requestBody.Should().Contain("\"name\":\"runtime-key\"");
        requestBody.Should().Contain("\"status\":\"enabled\"");
        response.Name.Should().Be("runtime-key");
        response.Status.Should().Be(ApiKeyStatus.Enabled);
        response.Id.Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task UpdateApiKeyAsync_PatchesStatusOnOrganizationScopedEndpoint()
    {
        OrganizationId organizationId = OrganizationId.From("550e8400-e29b-41d4-a716-446655440000");
        ApiKeyId apiKeyId = ApiKeyId.From("6f9619ff-8b86-4d01-b42d-00cf4fc964ff");
        string? requestBody = null;

        Mock<HttpMessageHandler> handler = new();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Patch &&
                    request.RequestUri!.PathAndQuery == $"/api/v0/organizations/{organizationId}/api-keys/{apiKeyId}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "_id": "6f9619ff-8b86-4d01-b42d-00cf4fc964ff",
                          "name": "runtime-key",
                          "status": "disabled"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            });

        ApiKeyClient sut = CreateClient(handler.Object);

        ApiKeyResponse response = await sut.UpdateApiKeyAsync(
            organizationId,
            apiKeyId,
            new UpdateApiKeyRequest
            {
                Status = ApiKeyStatus.Disabled
            });

        requestBody.Should().Contain("\"status\":\"disabled\"");
        response.Status.Should().Be(ApiKeyStatus.Disabled);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task ListApiKeysAsync_WithQueryParameters_AppendsTypedQueryString()
    {
        OrganizationId organizationId = OrganizationId.From("550e8400-e29b-41d4-a716-446655440000");

        Mock<HttpMessageHandler> handler = new();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri!.PathAndQuery == $"/api/v0/organizations/{organizationId}/api-keys?order_by=name&order=desc&status=enabled&limit=25&offset=50"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "data": [],
                      "count": 0,
                      "_type": "API_KEY_LIST"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });

        ApiKeyClient sut = CreateClient(handler.Object);

        ListApiKeyResponse response = await sut.ListApiKeysAsync(
            organizationId,
            new ListApiKeysQueryParameters
            {
                OrderBy = ApiKeySortField.Name,
                Order = SortDirection.Descending,
                Status = ApiKeyStatus.Enabled,
                Limit = 25,
                Offset = 50
            });

        response.Count.Should().Be(0);
        response.Data.Should().BeEmpty();
    }

    private ApiKeyClient CreateClient(HttpMessageHandler handler)
    {
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.fiskaly.com/api/v0/")
        };

        FiskalyHttpRequestExecutor executor = new(_jsonOptions, NullLogger<FiskalyHttpRequestExecutor>.Instance);
        return new ApiKeyClient(httpClient, executor, NullLogger<ApiKeyClient>.Instance, _jsonOptions);
    }
}
