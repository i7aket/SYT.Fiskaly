using System.Text.Json.Serialization;

namespace SYT.Fiskaly.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Algorithm
{
    [JsonStringEnumMemberName("ecdsa-plain-SHA256")]
    EcdsaPlainSha256
}
