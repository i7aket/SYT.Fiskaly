using System.Text.Json.Serialization;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Common.Enums;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.Serialization;

namespace SYT.Fiskaly.Management.ApiKeys.Responses;

public sealed class ApiKeyResponse
{
    [JsonPropertyName("_id")]
    public ApiKeyId? Id { get; init; }

    [JsonPropertyName("_type")]
    public string? Type { get; init; }

    [JsonPropertyName("_envs")]
    public List<Env>? Envs { get; init; }

    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("secret")]
    public string? Secret { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("status")]
    public ApiKeyStatus? Status { get; init; }

    [JsonPropertyName("managed_by_organization_id")]
    [JsonConverter(typeof(NullableUuidIdentifierOrEmptyStringJsonConverter<OrganizationId>))]
    public OrganizationId? ManagedByOrganizationId { get; init; }

    [JsonPropertyName("metadata")]
    public MetadataCollection? Metadata { get; init; }

    [JsonPropertyName("created_at")]
    public long? CreatedAtUnixSeconds { get; init; }

    [JsonPropertyName("created_by_user")]
    public Guid? CreatedByUser { get; init; }
}
