using System.Text.Json.Serialization;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.SignDE.Tss.Requests;

public class CreateTssRequest
{
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataCollection? Metadata { get; init; }
}
