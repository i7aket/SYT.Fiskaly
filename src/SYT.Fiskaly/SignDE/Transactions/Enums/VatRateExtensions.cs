using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.SignDE.Transactions.Enums;

public static class VatRateExtensions
{
    public static string ToApiString(this VatRate vatRate) =>
        EnumApiValueProvider.GetApiName(vatRate);
}
