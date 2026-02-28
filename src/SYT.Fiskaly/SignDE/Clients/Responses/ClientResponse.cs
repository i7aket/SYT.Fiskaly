using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Clients.Enums;
using SYT.Fiskaly.SignDE.Clients.ValueObjects;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Common.Enums;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.SignDE.Clients.Responses;

public class ClientResponse
{
    internal const string ExpectedResourceType = "CLIENT";
    [JsonPropertyName("_id")]
    public ClientId? Id { get; init; }
    [JsonPropertyName("serial_number")]
    public ClientSerialNumber? SerialNumber { get; init; }
    [JsonPropertyName("_env")]
    public Env? Env { get; init; }
    [JsonPropertyName("_type")]
    public ResourceType? Type { get; init; }
    [JsonPropertyName("_version")]
    public string? Version { get; init; }
    [JsonPropertyName("state")]
    public ClientState? State { get; init; }
    [JsonPropertyName("metadata")]
    public MetadataCollection? Metadata { get; init; }
    [JsonPropertyName("time_creation")]
    public DateTimeOffset? TimeCreation { get; init; }
    [JsonPropertyName("time_update")]
    public DateTimeOffset? TimeUpdate { get; init; }
    [JsonPropertyName("tss_id")]
    public TssId? TssId { get; init; }
}
