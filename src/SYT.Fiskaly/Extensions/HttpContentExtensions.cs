using System.Net.Http.Json;
using System.Text.Json;
using SYT.Fiskaly.Exceptions;

namespace SYT.Fiskaly.Extensions;

public static class HttpContentExtensions
{
    public static async Task<T> ReadFiskalyJsonAsync<T>(
        this HttpContent content,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(serializerOptions);

        T? result = await content.ReadFromJsonAsync<T>(serializerOptions, cancellationToken)
            .ConfigureAwait(false);

        if (result == null)
        {
            throw new FiskalyException($"Failed to deserialize JSON to type {typeof(T).Name}");
        }

        return result;
    }

    /// <summary>
    /// Deserializes the response AND hands back the exact bytes it came from, as text.
    ///
    /// <para>Separate from <see cref="ReadFiskalyJsonAsync{T}"/> rather than replacing it: that one streams
    /// straight into the deserializer, which is what large payloads such as export archives need. Buffering
    /// the body is only worth it where the caller genuinely wants the original - a fiscal signature, where an
    /// audit may later have to be shown what the provider actually returned rather than our reading of it.</para>
    /// </summary>
    public static async Task<(T Value, string RawJson)> ReadFiskalyJsonWithRawAsync<T>(
        this HttpContent content,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(serializerOptions);

        string rawJson = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        T? result = JsonSerializer.Deserialize<T>(rawJson, serializerOptions);

        if (result == null)
        {
            throw new FiskalyException($"Failed to deserialize JSON to type {typeof(T).Name}");
        }

        return (result, rawJson);
    }
}
