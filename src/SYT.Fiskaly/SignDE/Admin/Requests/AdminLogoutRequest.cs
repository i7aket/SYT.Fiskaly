using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Admin.Requests;

public sealed class AdminLogoutRequest
{
    [JsonIgnore]
    public static AdminLogoutRequest Empty { get; } = new();
}
