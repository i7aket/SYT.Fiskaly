using System.Text.Json;
using AwesomeAssertions;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Serialization;

namespace SYT.Fiskaly.UnitTests.Serialization;

public class NullableUuidIdentifierOrEmptyStringJsonConverterTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters =
        {
            new NullableUuidIdentifierOrEmptyStringJsonConverter<OrganizationId>()
        }
    };

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        OrganizationId? value = JsonSerializer.Deserialize<OrganizationId?>("null", JsonOptions);

        value.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_EmptyString_ReturnsNull()
    {
        OrganizationId? value = JsonSerializer.Deserialize<OrganizationId?>("\"\"", JsonOptions);

        value.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_WhitespaceString_ReturnsNull()
    {
        OrganizationId? value = JsonSerializer.Deserialize<OrganizationId?>("\"   \"", JsonOptions);

        value.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_ValidUuid_ReturnsTypedIdentifier()
    {
        const string organizationId = "9b8ad703-b85c-4dec-882d-2dc7525ada3f";

        OrganizationId? value = JsonSerializer.Deserialize<OrganizationId?>($"\"{organizationId}\"", JsonOptions);

        value.Should().Be(OrganizationId.From(organizationId));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Deserialize_InvalidUuid_ThrowsJsonException()
    {
        Action act = () => JsonSerializer.Deserialize<OrganizationId?>("\"not-a-guid\"", JsonOptions);

        act.Should().Throw<JsonException>()
            .WithMessage("*Invalid OrganizationId*");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_Null_WritesJsonNull()
    {
        string json = JsonSerializer.Serialize<OrganizationId?>(null, JsonOptions);

        json.Should().Be("null");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Serialize_Value_WritesUuidString()
    {
        OrganizationId value = OrganizationId.From("9b8ad703-b85c-4dec-882d-2dc7525ada3f");

        string json = JsonSerializer.Serialize<OrganizationId?>(value, JsonOptions);

        json.Should().Be("\"9b8ad703-b85c-4dec-882d-2dc7525ada3f\"");
    }
}
