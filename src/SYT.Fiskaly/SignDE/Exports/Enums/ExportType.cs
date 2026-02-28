using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Exports.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExportType
{
    [JsonStringEnumMemberName("EXPORT")]
    Export
}
