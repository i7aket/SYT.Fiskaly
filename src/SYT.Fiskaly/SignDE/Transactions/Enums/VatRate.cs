using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Transactions.Enums;

/// <summary>
/// VAT rate category for fiskaly SIGN DE transactions.
/// </summary>
/// <remarks>
/// The underlying numeric values (<c>1..5</c>) are intentionally aligned with DE DSFinV‑K
/// <c>UST_SCHLUESSEL</c> so exports can use <c>(int)VatRate</c>, while SIGN DE uses the
/// JSON string names (NORMAL/REDUCED_1/SPECIAL_RATE_1/SPECIAL_RATE_2/NULL).
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VatRate
{
    [JsonStringEnumMemberName("NORMAL")]
    Normal = 1,
    [JsonStringEnumMemberName("REDUCED_1")]
    Reduced1 = 2,
    [JsonStringEnumMemberName("SPECIAL_RATE_1")]
    SpecialRate1 = 3,
    [JsonStringEnumMemberName("SPECIAL_RATE_2")]
    SpecialRate2 = 4,
    [JsonStringEnumMemberName("NULL")]
    Null = 5
}
