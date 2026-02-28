using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.SignDE.Transactions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionSortField
{
    [JsonStringEnumMemberName("number")]
    Number = 0,
    [JsonStringEnumMemberName("state")]
    State,
    [JsonStringEnumMemberName("time_start")]
    TimeStart,
    [JsonStringEnumMemberName("time_end")]
    TimeEnd
}

public static class TransactionSortFieldExtensions
{
    public static string ToApiString(this TransactionSortField field) =>
        EnumApiValueProvider.GetApiName(field);
}
