using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SYT.Fiskaly.Authentication.ValueObjects;

[JsonConverter(typeof(ApiSecretJsonConverter))]
public readonly partial record struct ApiSecret : IParsable<ApiSecret>
{
    private const int MinimumLength = 6;

    private const int MaximumLength = 512;

    [GeneratedRegex(@".*[^\s].*")]
    private static partial Regex ValidationPattern();

    public string Value { get; }

    // The shape of a fiskaly API secret is fiskaly's business, not ours. This used to demand exactly 43
    // alphanumeric characters - a rule taken from one vendor analysis and enforced as if it were a
    // contract - and fiskaly then issued a managed-organisation secret of 42, which made every
    // provisioning call fail with a FormatException blamed on the caller. The same rule also rejected
    // underscores while the error message it threw advertised the format "test_xxx_xxx", so the
    // validation contradicted its own explanation.
    //
    // A credential we did not mint is checked for the only things we can honestly assert about it: that
    // something is there, and that it is not absurdly long. Those are exactly the bounds the sibling
    // ApiKey in this folder already uses, and they are shared with it deliberately - one family, one
    // rule. A truncated secret now fails at fiskaly with a 401 instead of locally with a wrong reason,
    // which is the correct place for the vendor to reject its own credential.
    private ApiSecret(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        string trimmed = value.Trim();

        if (Rejection(trimmed) is { } rejection)
        {
            throw new FormatException(rejection);
        }

        Value = trimmed;
    }

    /// <summary>
    /// Why a trimmed candidate is not a usable secret, or null when it is. ONE implementation, because
    /// <see cref="TryFrom"/> is the other entry point and used to carry its own copy of the rule - which is
    /// exactly how the two drifted: a bound added here alone left TryFrom throwing out of a method whose
    /// whole contract is that it does not throw.
    /// </summary>
    private static string? Rejection(string trimmed) => trimmed.Length switch
    {
        < MinimumLength =>
            $"API secret must be at least {MinimumLength} characters long. " +
            $"Current: {trimmed.Length} characters. Verify your fiskaly credentials.",
        > MaximumLength =>
            $"API secret must not exceed {MaximumLength} characters. " +
            $"Current: {trimmed.Length} characters. Verify your fiskaly credentials.",
        _ => ValidationPattern().IsMatch(trimmed)
            ? null
            : "API secret must contain at least one non-whitespace character. " +
              "Current value appears to be only whitespace.",
    };

    public static ApiSecret From(string value) => new(value);

    public static bool TryFrom(string? value, out ApiSecret secret)
    {
        secret = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();

        if (Rejection(trimmed) is not null)
        {
            return false;
        }

        secret = new ApiSecret(trimmed);
        return true;
    }

    public static ApiSecret Parse(string s, IFormatProvider? provider)
    {
        return From(s);
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out ApiSecret result)
    {
        return TryFrom(s, out result);
    }

    public override string ToString()
    {
        return new string('*', Math.Min(Value.Length, 8));
    }

    private sealed class ApiSecretJsonConverter : JsonConverter<ApiSecret>
    {
        public override ApiSecret Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (value is null)
            {
                throw new JsonException("Expected non-null string for API secret.");
            }

            return From(value);
        }

        public override void Write(Utf8JsonWriter writer, ApiSecret value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
