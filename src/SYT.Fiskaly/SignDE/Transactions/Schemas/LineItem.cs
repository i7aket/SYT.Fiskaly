using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Transactions.Serialization;
using SYT.Fiskaly.SignDE.Transactions.ValueObjects;

namespace SYT.Fiskaly.SignDE.Transactions.Schemas;

public class LineItem
{
    private const int MaxTextLength = 255;

    private readonly string _text = string.Empty;
    [JsonPropertyName("quantity")]
    [JsonConverter(typeof(DecimalToStringJsonConverter))]
    public required decimal Quantity { get; init; }
    [JsonPropertyName("text")]
    public required string Text
    {
        get => _text;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Text cannot be null or whitespace.",
                    nameof(value));
            }

            if (value.Length > MaxTextLength)
            {
                throw new ArgumentException(
                    $"Text cannot exceed {MaxTextLength} characters. Provided text has {value.Length} characters.",
                    nameof(value));
            }
            _text = value;
        }
    }
    [JsonPropertyName("price_per_unit")]
    public required MoneyAmount PricePerUnit { get; init; }
}
