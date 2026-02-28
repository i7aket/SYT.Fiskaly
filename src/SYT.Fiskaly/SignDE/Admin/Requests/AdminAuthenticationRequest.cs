using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Admin.ValueObjects;

namespace SYT.Fiskaly.SignDE.Admin.Requests;

public sealed class AdminAuthenticationRequest
{
    [JsonPropertyName("admin_pin")]
    public required AdminPin AdminPin { get; init; }
}
