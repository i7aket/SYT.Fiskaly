using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.SignDE.Transactions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TxState
{
    [JsonStringEnumMemberName("ACTIVE")]
    Active = 0,
    [JsonStringEnumMemberName("FINISHED")]
    Finished,
    [JsonStringEnumMemberName("CANCELLED")]
    Cancelled
}

public static class TxStateExtensions
{
    public static string ToApiString(this TxState state) =>
        EnumApiValueProvider.GetApiName(state);

    public static string? ToApiString(this TxState? state) =>
        state.HasValue ? state.Value.ToApiString() : null;
}
