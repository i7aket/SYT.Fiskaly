using System.Text.Json.Serialization;
using SYT.Fiskaly.Authentication.ValueObjects;

namespace SYT.Fiskaly.Management.Organizations.Models;

public class BillingOptions
{
    [JsonPropertyName("gln")]
    public string? Gln { get; init; }
    [JsonPropertyName("withhold_billing")]
    public bool? WithholdBilling { get; init; }
    [JsonPropertyName("bill_to_organization")]
    public OrganizationId? BillToOrganization { get; init; }
}
