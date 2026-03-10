using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.Management.Common.Responses;
using SYT.Fiskaly.Management.Common.Enums;
using SYT.Fiskaly.Management.Organizations;
using SYT.Fiskaly.Management.Organizations.Models;
using SYT.Fiskaly.Management.Organizations.Requests;
using SYT.Fiskaly.Management.Organizations.Responses;
using SYT.Fiskaly.Common.Enums;

namespace SYT.Fiskaly.UnitTests.Management.Organizations;

public class OrganizationClientManagementTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    [Trait("Category", "Unit")]
    [Fact]
    public async Task CreateOrganizationAsync_PostsToOrganizationsEndpoint()
    {
        string? requestBody = null;

        Mock<HttpMessageHandler> handler = new();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.RequestUri!.PathAndQuery == "/api/v0/organizations"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "_id": "550e8400-e29b-41d4-a716-446655440000",
                          "name": "Pizza Nostra Berlin",
                          "address_line1": "Lychener Str. 2",
                          "zip": "10437",
                          "town": "Berlin",
                          "country_code": "DEU"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            });

        OrganizationClient sut = CreateClient(handler.Object);

        OrganizationResponse response = await sut.CreateOrganizationAsync(new CreateOrganizationRequest
        {
            Name = "Pizza Nostra Berlin",
            AddressLine1 = "Lychener Str. 2",
            Zip = "10437",
            Town = "Berlin",
            CountryCode = CountryCode.DEU,
            ManagedByOrganizationId = OrganizationId.From("123e4567-e89b-42d3-a456-426614174000")
        });

        requestBody.Should().Contain("\"name\":\"Pizza Nostra Berlin\"");
        requestBody.Should().Contain("\"country_code\":\"DEU\"");
        requestBody.Should().Contain("\"managed_by_organization_id\":\"123e4567-e89b-42d3-a456-426614174000\"");
        response.Name.Should().Be("Pizza Nostra Berlin");
        response.Id.Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task EnableEnvironmentAsync_PostsEnableEnvRequest()
    {
        OrganizationId organizationId = OrganizationId.From("550e8400-e29b-41d4-a716-446655440000");
        string? requestBody = null;

        Mock<HttpMessageHandler> handler = new();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.RequestUri!.PathAndQuery == $"/api/v0/organizations/{organizationId}/enable-env"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "_success": true
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            });

        OrganizationClient sut = CreateClient(handler.Object);

        StatusResponse response = await sut.EnableEnvironmentAsync(organizationId, Env.Test);

        requestBody.Should().Contain("\"env\":\"TEST\"");
        response.Success.Should().BeTrue();
    }

    private OrganizationClient CreateClient(HttpMessageHandler handler)
    {
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.fiskaly.com/api/v0/")
        };

        FiskalyHttpRequestExecutor executor = new(_jsonOptions, NullLogger<FiskalyHttpRequestExecutor>.Instance);
        return new OrganizationClient(httpClient, executor, NullLogger<OrganizationClient>.Instance, _jsonOptions);
    }
}
