using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Transactions.Schemas;

public class Order : StandardV1SchemaPayload
{
    [JsonPropertyName("line_items")]
    public required List<LineItem> LineItems { get; init; }
}
