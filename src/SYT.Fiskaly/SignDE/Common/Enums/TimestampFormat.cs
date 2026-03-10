using System.Text.Json.Serialization;

namespace SYT.Fiskaly.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TimestampFormat
{
    [JsonStringEnumMemberName("unixTime")]
    UnixTime
}
