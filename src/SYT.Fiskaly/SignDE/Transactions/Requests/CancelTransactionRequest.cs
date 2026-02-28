using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Transactions.Enums;
using SYT.Fiskaly.SignDE.Transactions.Schemas;

namespace SYT.Fiskaly.SignDE.Transactions.Requests;

public sealed class CancelTransactionRequest : TxRequest
{
    [SetsRequiredMembers]
    public CancelTransactionRequest()
    {
        State = TxState.Cancelled;
    }
    [JsonPropertyName("schema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransactionSchema? Schema { get; init; }
}
