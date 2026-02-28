using System.Diagnostics.CodeAnalysis;
using SYT.Fiskaly.SignDE.Transactions.Enums;

namespace SYT.Fiskaly.SignDE.Transactions.Requests;

public class StartTransactionRequest : TxRequest
{
    [SetsRequiredMembers]
    public StartTransactionRequest()
    {
        State = TxState.Active;
    }
}
