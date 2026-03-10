using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Management.ApiKeys.Responses;

namespace SYT.Fiskaly.UnitTests.Management.ApiKeys;

public class ApiKeyResponseTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_EmptyManagedByOrganizationId_DoesNotThrow()
    {
        const string json = """
                            {
                              "_id": "550e8400-e29b-41d4-a716-446655440000",
                              "name": "runtime-key",
                              "managed_by_organization_id": ""
                            }
                            """;

        ApiKeyResponse? response = JsonSerializer.Deserialize<ApiKeyResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Name.Should().Be("runtime-key");
        response.ManagedByOrganizationId.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_NullManagedByOrganizationId_ReturnsNull()
    {
        const string json = """
                            {
                              "_id": "550e8400-e29b-41d4-a716-446655440000",
                              "managed_by_organization_id": null
                            }
                            """;

        ApiKeyResponse? response = JsonSerializer.Deserialize<ApiKeyResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.ManagedByOrganizationId.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_ValidManagedByOrganizationId_ReturnsTypedOrganizationId()
    {
        const string managedByOrganizationId = "550e8400-e29b-41d4-a716-446655440001";
        string json = $$"""
                        {
                          "_id": "550e8400-e29b-41d4-a716-446655440000",
                          "managed_by_organization_id": "{{managedByOrganizationId}}"
                        }
                        """;

        ApiKeyResponse? response = JsonSerializer.Deserialize<ApiKeyResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.ManagedByOrganizationId.Should().Be(OrganizationId.From(managedByOrganizationId));
    }
}
