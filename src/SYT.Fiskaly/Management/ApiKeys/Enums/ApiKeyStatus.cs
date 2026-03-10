using System.Text.Json.Serialization;

namespace SYT.Fiskaly.Management.ApiKeys.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiKeyStatus
{
    [JsonStringEnumMemberName("enabled")]
    Enabled,
    [JsonStringEnumMemberName("disabled")]
    Disabled
}
