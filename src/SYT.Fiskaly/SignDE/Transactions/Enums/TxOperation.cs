using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Transactions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TxOperation
{
    [JsonStringEnumMemberName("Start")]
    Start,
    [JsonStringEnumMemberName("Update")]
    Update,
    [JsonStringEnumMemberName("Finish")]
    Finish
}
