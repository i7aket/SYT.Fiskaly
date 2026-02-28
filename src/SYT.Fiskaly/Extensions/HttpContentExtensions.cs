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
}
