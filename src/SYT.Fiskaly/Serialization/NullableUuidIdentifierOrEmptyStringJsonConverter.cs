using System.Text.Json;
using System.Text.Json.Serialization;
using SYT.Fiskaly.ValueObjects;

namespace SYT.Fiskaly.Serialization;

public sealed class NullableUuidIdentifierOrEmptyStringJsonConverter<T> : JsonConverter<T?>
    where T : struct, IUuidIdentifier<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string or null token for {typeof(T).Name}, got {reader.TokenType}.");
        }

        string? value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return T.From(value);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException($"Invalid {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString());
    }
}
