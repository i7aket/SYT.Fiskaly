using System.Text.Json.Serialization;
using SYT.Fiskaly.Common.Enums;

namespace SYT.Fiskaly.Management.Organizations.Requests;

public sealed class EnableOrganizationEnvironmentRequest
{
    [JsonPropertyName("env")]
    public required Env Env { get; init; }
}
