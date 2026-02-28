using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.SignDE.Transactions.Enums;

public static class ReceiptTypeExtensions
{
    public static string ToApiString(this ReceiptType receiptType) =>
        EnumApiValueProvider.GetApiName(receiptType);
}
