using System.Text.Json.Serialization;
using SYT.Fiskaly.Serialization;
using SYT.Fiskaly.ValueObjects;

namespace SYT.Fiskaly.Authentication.ValueObjects;

[JsonConverter(typeof(UuidIdentifierJsonConverterFactory))]
public readonly partial record struct ApiKeyId : IUuidIdentifier<ApiKeyId>, IParsable<ApiKeyId>
{
    public Guid Value { get; }

    private ApiKeyId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("API key identifier cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static ApiKeyId New() => new(Guid.NewGuid());

    public static ApiKeyId From(string uuid)
        => UuidIdentifierHelper.From(uuid, value => new ApiKeyId(value), "API key identifier");

    public static bool TryParse(string? value, out ApiKeyId result)
        => TryParse(value, null, out result);

    public static ApiKeyId Parse(string s, IFormatProvider? provider)
        => UuidIdentifierHelper.Parse(s, provider, value => new ApiKeyId(value), "API key identifier");

    public static bool TryParse(string? s, IFormatProvider? provider, out ApiKeyId result)
    {
        result = default;

        try
        {
            return UuidIdentifierHelper.TryParse(s, provider, value => new ApiKeyId(value), out result);
        }
        catch (ArgumentException)
        {
            result = default;
            return false;
        }
    }

    public override string ToString() => Value.ToString();
}
