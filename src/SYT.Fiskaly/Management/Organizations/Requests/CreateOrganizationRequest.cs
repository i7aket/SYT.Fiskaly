using System.Text.Json.Serialization;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Management.Common.Enums;
using SYT.Fiskaly.Management.Organizations.Models;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.Management.Organizations.Requests;

public sealed class CreateOrganizationRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("address_line1")]
    public required string AddressLine1 { get; init; }

    [JsonPropertyName("zip")]
    public required string Zip { get; init; }

    [JsonPropertyName("town")]
    public required string Town { get; init; }

    [JsonPropertyName("country_code")]
    public required CountryCode CountryCode { get; init; }

    [JsonPropertyName("display_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; init; }

    [JsonPropertyName("vat_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VatId { get; init; }

    [JsonPropertyName("contact_person_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ContactPersonId { get; init; }

    [JsonPropertyName("address_line2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLine2 { get; init; }

    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? State { get; init; }

    [JsonPropertyName("tax_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaxNumber { get; init; }

    [JsonPropertyName("economy_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EconomyId { get; init; }

    [JsonPropertyName("billing_options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BillingOptions? BillingOptions { get; init; }

    [JsonPropertyName("billing_address_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? BillingAddressId { get; init; }

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataCollection? Metadata { get; init; }

    [JsonPropertyName("managed_by_organization_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OrganizationId? ManagedByOrganizationId { get; init; }

    [Obsolete("Use BillingOptions instead. This field is kept for compatibility with Management API v0.")]
    [JsonPropertyName("managed_configuration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ManagedOrganizationConfiguration? ManagedConfiguration { get; init; }
}
