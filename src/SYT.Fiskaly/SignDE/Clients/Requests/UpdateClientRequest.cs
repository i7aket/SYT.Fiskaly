using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Clients.Enums;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.SignDE.Clients.Requests;

public sealed class UpdateClientRequest
{
    [JsonPropertyName("state")]
    public required ClientState State { get; init; }
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataCollection? Metadata { get; init; }

    public static UpdateClientRequest Register(MetadataCollection? metadata = null)
    {
        return new UpdateClientRequest { State = ClientState.Registered, Metadata = metadata };
    }

    public static UpdateClientRequest Deregister(MetadataCollection? metadata = null)
    {
        return new UpdateClientRequest { State = ClientState.Deregistered, Metadata = metadata };
    }
}
