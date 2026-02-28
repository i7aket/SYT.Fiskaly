using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Transactions.Enums;
using SYT.Fiskaly.SignDE.Transactions.ValueObjects;

namespace SYT.Fiskaly.SignDE.Transactions.Schemas;

public class PaymentTypeAmount
{
    [JsonPropertyName("payment_type")]
    public PaymentType PaymentType { get; init; }
    [JsonPropertyName("amount")]
    public MoneyAmount Amount { get; init; } = MoneyAmount.Zero(global::SYT.Fiskaly.SignDE.Common.CurrencyCode.EUR);

    private readonly string? _currencyCode;

    [JsonPropertyName("currency_code")]
    public string CurrencyCode
    {
        get => _currencyCode ?? Amount.CurrencyIsoCode;
        init => _currencyCode = value;
    }

    [JsonIgnore]
    public CurrencyCode EffectiveCurrency =>
        _currencyCode is { Length: > 0 }
            ? CurrencyCodeExtensions.ParseIsoString(_currencyCode)
            : Amount.Currency;
}
