using System.Text.Json.Serialization;
using SYT.Fiskaly.Management.ApiKeys.Enums;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.Management.ApiKeys.Requests;

public sealed class UpdateApiKeyRequest
{
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiKeyStatus? Status { get; init; }

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataCollection? Metadata { get; init; }
}
