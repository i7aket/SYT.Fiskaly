using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Transactions.Enums;
using SYT.Fiskaly.SignDE.Transactions.ValueObjects;

namespace SYT.Fiskaly.SignDE.Transactions.Schemas;

public class VatRateAmount
{
    [JsonPropertyName("vat_rate")]
    public VatRate VatRate { get; init; }
    [JsonPropertyName("amount")]
    public MoneyAmount Amount { get; init; } = MoneyAmount.Zero(CurrencyCode.EUR);
}
