using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Admin.ValueObjects;

namespace SYT.Fiskaly.SignDE.Admin.Requests;

public class ChangeAdminPinRequest
{
    [JsonPropertyName("admin_puk")]
    public required AdminPuk AdminPuk { get; init; }
    [JsonPropertyName("new_admin_pin")]
    public required AdminPin NewAdminPin { get; init; }
}
