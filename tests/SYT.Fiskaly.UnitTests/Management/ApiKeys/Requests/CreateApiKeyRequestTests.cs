using System.Text.Json;
using AwesomeAssertions;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.UnitTests.Management.ApiKeys.Requests;

public class CreateApiKeyRequestTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_WithOptionalValues_WritesExpectedPayload()
    {
        CreateApiKeyRequest request = new()
        {
            Name = "runtime-key",
            Status = ApiKeyStatus.Enabled,
            Metadata = MetadataCollection.Empty.Add("location", "Berlin"),
            ManagedByOrganizationId = OrganizationId.From("9b8ad703-b85c-4dec-882d-2dc7525ada3f")
        };

        string json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"name\":\"runtime-key\"");
        json.Should().Contain("\"status\":\"enabled\"");
        json.Should().Contain("\"managed_by_organization_id\":\"9b8ad703-b85c-4dec-882d-2dc7525ada3f\"");
        json.Should().Contain("\"location\":\"Berlin\"");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_WithNullOptionalValues_OmitsNullProperties()
    {
        CreateApiKeyRequest request = new()
        {
            Name = "runtime-key"
        };

        string json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"name\":\"runtime-key\"");
        json.Should().NotContain("managed_by_organization_id");
        json.Should().NotContain("\"metadata\":");
    }
}
