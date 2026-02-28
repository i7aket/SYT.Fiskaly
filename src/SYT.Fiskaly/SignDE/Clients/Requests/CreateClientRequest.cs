using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Clients.ValueObjects;

namespace SYT.Fiskaly.SignDE.Clients.Requests;

public class CreateClientRequest
{
    [JsonPropertyName("serial_number")]
    public required ClientSerialNumber SerialNumber { get; init; }
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataCollection? Metadata { get; init; }
}
