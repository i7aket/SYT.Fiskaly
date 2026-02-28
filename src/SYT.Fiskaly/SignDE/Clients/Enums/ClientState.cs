using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Clients.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClientState
{
    [JsonStringEnumMemberName("REGISTERED")]
    Registered,
    [JsonStringEnumMemberName("DEREGISTERED")]
    Deregistered
}
