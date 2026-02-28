using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.SignDE.Admin.Responses;

public class AdminAuthenticationResponse
{
    [JsonIgnore]
    public TssId TssId { get; init; }
}
