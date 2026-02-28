using System.Text.Json.Serialization;

namespace SYT.Fiskaly.Management.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganizationType
{
    [JsonStringEnumMemberName("ORGANIZATION")]
    Organization,
    [JsonStringEnumMemberName("MANAGED_ORGANIZATION")]
    ManagedOrganization
}
