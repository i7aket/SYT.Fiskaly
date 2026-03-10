using System.Text.Json.Serialization;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.Management.ApiKeys.Requests;

public sealed class CreateApiKeyRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("status")]
    public ApiKeyStatus Status { get; init; } = ApiKeyStatus.Enabled;

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataCollection? Metadata { get; init; }

    [JsonPropertyName("managed_by_organization_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OrganizationId? ManagedByOrganizationId { get; init; }
}
