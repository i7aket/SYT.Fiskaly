using System.Text.Json;
using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Transactions.Responses;

namespace SYT.Fiskaly.UnitTests.SignDE.Transactions.Responses;

/// <summary>
/// RawJson exists so a German fiscal signature can be shown to an auditor as the provider returned it. It is
/// carried alongside the parsed response, never part of the wire contract - if it ever serialized, the SDK
/// would be sending the provider a copy of its own previous answer, and a round-trip would nest bodies inside
/// bodies.
/// </summary>
public class TxResponseRawJsonTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Trait("Category", "Unit")]
    [Fact]
    public void RawJson_IsNeverWrittenWhenSerializing()
    {
        TxResponse response = new() { RawJson = """{"secret":"must-not-be-echoed"}""" };

        string serialized = JsonSerializer.Serialize(response, _jsonOptions);

        Assert.DoesNotContain("RawJson", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("must-not-be-echoed", serialized, StringComparison.Ordinal);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RawJson_IsNeverReadWhenDeserializing()
    {
        // A provider that happened to return a field of this name must not be able to set it.
        string json = """{"number":7,"rawJson":"injected"}""";

        TxResponse? response = JsonSerializer.Deserialize<TxResponse>(json, _jsonOptions);

        Assert.NotNull(response);
        Assert.Equal(7, response!.Number);
        Assert.Null(response.RawJson);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RawJson_DefaultsToNull_ForResponsesThatCameThroughAPathWhichDidNotBufferTheBody()
    {
        // List responses and any pre-existing call path still deserialize without the body; null is the honest
        // answer there, and callers are documented to expect it.
        TxResponse? response = JsonSerializer.Deserialize<TxResponse>("""{"number":1}""", _jsonOptions);

        Assert.NotNull(response);
        Assert.Null(response!.RawJson);
    }
}
