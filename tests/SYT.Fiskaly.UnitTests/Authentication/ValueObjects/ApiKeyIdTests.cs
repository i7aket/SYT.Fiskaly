using System.Text.Json;
using AwesomeAssertions;
using SYT.Fiskaly.Authentication.ValueObjects;

namespace SYT.Fiskaly.UnitTests.Authentication.ValueObjects;

public class ApiKeyIdTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void New_CreatesNonEmptyIdentifier()
    {
        ApiKeyId apiKeyId = ApiKeyId.New();

        apiKeyId.Value.Should().NotBe(Guid.Empty);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_ValidUuidV4String_ReturnsApiKeyId()
    {
        const string uuid = "550e8400-e29b-41d4-a716-446655440000";

        ApiKeyId apiKeyId = JsonSerializer.Deserialize<ApiKeyId>($"\"{uuid}\"");

        apiKeyId.Value.Should().Be(Guid.Parse(uuid));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_InvalidString_ThrowsJsonException()
    {
        Action act = () => JsonSerializer.Deserialize<ApiKeyId>("\"not-a-guid\"");

        act.Should().Throw<JsonException>();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_WritesLowercaseUuidString()
    {
        const string uuid = "550e8400-e29b-41d4-a716-446655440000";
        ApiKeyId apiKeyId = ApiKeyId.From(uuid);

        string json = JsonSerializer.Serialize(apiKeyId);

        json.Should().Be($"\"{uuid}\"");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_EmptyGuid_ThrowsArgumentException()
    {
        Action act = () => ApiKeyId.From(Guid.Empty.ToString());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid UUIDv4 format*");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TryParse_EmptyGuid_ReturnsFalse()
    {
        bool result = ApiKeyId.TryParse(Guid.Empty.ToString(), out ApiKeyId apiKeyId);

        result.Should().BeFalse();
        apiKeyId.Value.Should().Be(Guid.Empty);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Parse_ValidString_ReturnsIdentifier()
    {
        const string uuid = "550e8400-e29b-41d4-a716-446655440000";

        ApiKeyId apiKeyId = ApiKeyId.Parse(uuid, provider: null);

        apiKeyId.Should().Be(ApiKeyId.From(uuid));
    }
}
