using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Exports.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExportSortField
{
    [JsonStringEnumMemberName("state")]
    State,
    [JsonStringEnumMemberName("time_request")]
    TimeRequest,
    [JsonStringEnumMemberName("time_start")]
    TimeStart,
    [JsonStringEnumMemberName("time_end")]
    TimeEnd,
    [JsonStringEnumMemberName("time_expiration")]
    TimeExpiration,
    [JsonStringEnumMemberName("time_error")]
    TimeError
}
