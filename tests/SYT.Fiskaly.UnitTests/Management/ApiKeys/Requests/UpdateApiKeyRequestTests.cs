using System.Text.Json;
using AwesomeAssertions;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.UnitTests.Management.ApiKeys.Requests;

public class UpdateApiKeyRequestTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_WithOptionalValues_WritesExpectedPayload()
    {
        UpdateApiKeyRequest request = new()
        {
            Status = ApiKeyStatus.Disabled,
            Metadata = MetadataCollection.Empty.Add("disabled", "true")
        };

        string json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"status\":\"disabled\"");
        json.Should().Contain("\"disabled\":\"true\"");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_WithNullOptionalValues_OmitsNullProperties()
    {
        UpdateApiKeyRequest request = new();

        string json = JsonSerializer.Serialize(request);

        json.Should().Be("{}");
    }
}
