using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Tss.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TssState
{
    [JsonStringEnumMemberName("CREATED")]
    Created,
    [JsonStringEnumMemberName("UNINITIALIZED")]
    Uninitialized,
    [JsonStringEnumMemberName("INITIALIZED")]
    Initialized,
    [JsonStringEnumMemberName("DISABLED")]
    Disabled,
    [JsonStringEnumMemberName("DELETED")]
    Deleted,
    [JsonStringEnumMemberName("DEFECTIVE")]
    Defective,
    [JsonStringEnumMemberName("EVICTED")]
    Evicted
}
