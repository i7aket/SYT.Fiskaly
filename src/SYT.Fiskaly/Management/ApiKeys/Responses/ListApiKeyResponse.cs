using System.Text.Json.Serialization;

namespace SYT.Fiskaly.Management.ApiKeys.Responses;

public sealed class ListApiKeyResponse
{
    [JsonPropertyName("data")]
    public List<ApiKeyResponse> Data { get; init; } = [];

    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("_type")]
    public string? Type { get; init; }
}
