using System.Text.Json.Serialization;

namespace SYT.Fiskaly.Management.ApiKeys.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiKeySortField
{
    [JsonStringEnumMemberName("name")]
    Name,
    [JsonStringEnumMemberName("status")]
    Status,
    [JsonStringEnumMemberName("created_at")]
    CreatedAt
}
