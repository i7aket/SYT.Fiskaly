using System.Text.Json;
using System.Net.Http.Json;
using SYT.Fiskaly.Extensions;

namespace SYT.Fiskaly.Http;

public class FiskalyHttpRequestExecutor
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<FiskalyHttpRequestExecutor> _logger;

    public FiskalyHttpRequestExecutor(
        JsonSerializerOptions serializerOptions,
        ILogger<FiskalyHttpRequestExecutor> logger)
    {
        _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> ExecuteGetAsync<TResponse>(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        _logger.LogExecutingGet(url);

        using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken)
            .ConfigureAwait(false);

        return await response.Content.ReadFiskalyJsonAsync<TResponse>(
            _serializerOptions,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResponse> ExecutePutAsync<TResponse>(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        _logger.LogExecutingPutNoBody(url);

        using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Put, url);

        using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        return await response.Content.ReadFiskalyJsonAsync<TResponse>(
            _serializerOptions,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<TResponse> ExecutePutAsync<TRequest, TResponse>(
        HttpClient httpClient,
        string url,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteJsonRequestAsync<TRequest, TResponse>(httpClient, HttpMethod.Put, url, request, cancellationToken);

    public Task<TResponse> ExecutePatchAsync<TRequest, TResponse>(
        HttpClient httpClient,
        string url,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteJsonRequestAsync<TRequest, TResponse>(httpClient, HttpMethod.Patch, url, request, cancellationToken);

    public Task<TResponse> ExecutePostAsync<TRequest, TResponse>(
        HttpClient httpClient,
        string url,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteJsonRequestAsync<TRequest, TResponse>(httpClient, HttpMethod.Post, url, request, cancellationToken);

    public async Task ExecutePostAsync<TRequest>(
        HttpClient httpClient,
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogExecutingPostNoResponse(url);

        using JsonContent content = JsonContent.Create(request, options: _serializerOptions);

        using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TResponse> ExecuteDeleteAsync<TResponse>(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        _logger.LogExecutingDelete(url);

        using HttpResponseMessage response = await httpClient.DeleteAsync(url, cancellationToken)
            .ConfigureAwait(false);

        return await response.Content.ReadFiskalyJsonAsync<TResponse>(
            _serializerOptions,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task ExecuteDeleteAsync(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        _logger.LogExecutingDelete(url);

        using HttpResponseMessage response = await httpClient.DeleteAsync(url, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// GET that also returns the untouched response body.
    ///
    /// <para>Added for the SIGN DE recovery path: after a lost connection the signature is re-observed with a
    /// GET rather than a FINISH, so an implementation that only captured the body on the write path would end
    /// up with nothing recorded in exactly the case an audit is most likely to ask about.</para>
    /// </summary>
    public async Task<(TResponse Value, string RawJson)> ExecuteGetWithRawAsync<TResponse>(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        _logger.LogExecutingGet(url);

        using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken)
            .ConfigureAwait(false);

        return await response.Content.ReadFiskalyJsonWithRawAsync<TResponse>(
            _serializerOptions,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// PUT with a body that also returns the untouched response body. See
    /// <see cref="ExecuteGetWithRawAsync{TResponse}"/> for why this is a separate method rather than a change
    /// to the existing one.
    /// </summary>
    public async Task<(TResponse Value, string RawJson)> ExecutePutWithRawAsync<TRequest, TResponse>(
        HttpClient httpClient,
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogExecutingRequest(HttpMethod.Put, url);

        using JsonContent content = JsonContent.Create(request, options: _serializerOptions);

        using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = content
        };

        using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        return await response.Content.ReadFiskalyJsonWithRawAsync<TResponse>(
            _serializerOptions,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResponse> ExecuteJsonRequestAsync<TRequest, TResponse>(
        HttpClient httpClient,
        HttpMethod method,
        string url,
        TRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogExecutingRequest(method, url);

        using JsonContent content = JsonContent.Create(request, options: _serializerOptions);

        using HttpRequestMessage httpRequest = new HttpRequestMessage(method, url)
        {
            Content = content
        };

        using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        return await response.Content.ReadFiskalyJsonAsync<TResponse>(
            _serializerOptions,
            cancellationToken).ConfigureAwait(false);
    }


}
