using System.Text.Json.Serialization;

namespace SYT.Fiskaly.SignDE.Tss.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedUpdateVariants
{
    [JsonStringEnumMemberName("SIGNED")]
    Signed
}
