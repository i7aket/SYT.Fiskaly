using System.Text.Json.Serialization;
using SYT.Fiskaly.Common.Enums;

namespace SYT.Fiskaly.SignDE.Tss.Responses;

public class ListTssResponse
{
    internal const string ExpectedResourceType = "TSS_LIST";
    [JsonPropertyName("data")]
    public List<TssResponse>? Data { get; init; }
    [JsonPropertyName("count")]
    public int? Count { get; init; }
    [JsonPropertyName("_type")]
    public ResourceType? Type { get; init; }
    [JsonPropertyName("_env")]
    public Env? Env { get; init; }
    [JsonPropertyName("_version")]
    public string? Version { get; init; }
}
