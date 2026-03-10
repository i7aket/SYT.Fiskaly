using System.Text.Json;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.Management.ApiKeys.Models;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.Management.ApiKeys.Responses;
using SYT.Fiskaly.Management.Common.Responses;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.Management.ApiKeys;

public sealed class ApiKeyClient(
    HttpClient httpClient,
    FiskalyHttpRequestExecutor executor,
    ILogger<ApiKeyClient> logger,
    JsonSerializerOptions serializerOptions) : IApiKeyClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly FiskalyHttpRequestExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    private readonly ILogger<ApiKeyClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

    public async Task<ListApiKeyResponse> ListApiKeysAsync(
        OrganizationId organizationId,
        ListApiKeysQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        string url = queryParameters?.BuildUrl($"organizations/{organizationId}/api-keys")
            ?? $"organizations/{organizationId}/api-keys";

        _logger.LogDebug("Listing API keys for organization {OrganizationId} with URL: {Url}", organizationId.Value, url);

        ListApiKeyResponse response = await _executor.ExecuteGetAsync<ListApiKeyResponse>(
            _httpClient,
            url,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Retrieved {Count} API keys for organization {OrganizationId}",
            response.Count ?? response.Data.Count,
            organizationId.Value);

        return response;
    }

    public async Task<ApiKeyResponse> GetApiKeyAsync(
        OrganizationId organizationId,
        ApiKeyId apiKeyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Retrieving API key {ApiKeyId} for organization {OrganizationId}",
            apiKeyId.Value,
            organizationId.Value);

        return await _executor.ExecuteGetAsync<ApiKeyResponse>(
            _httpClient,
            $"organizations/{organizationId}/api-keys/{apiKeyId}",
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<ApiKeyResponse> CreateApiKeyAsync(
        OrganizationId organizationId,
        CreateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Creating API key for organization {OrganizationId} with name {Name}",
            organizationId.Value,
            request.Name);

        ApiKeyResponse response = await _executor.ExecutePostAsync<CreateApiKeyRequest, ApiKeyResponse>(
            _httpClient,
            $"organizations/{organizationId}/api-keys",
            request,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Created API key {ApiKeyId} for organization {OrganizationId}",
            response.Id?.Value,
            organizationId.Value);

        return response;
    }

    public async Task<ApiKeyResponse> UpdateApiKeyAsync(
        OrganizationId organizationId,
        ApiKeyId apiKeyId,
        UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Updating API key {ApiKeyId} for organization {OrganizationId}",
            apiKeyId.Value,
            organizationId.Value);

        return await _executor.ExecutePatchAsync<UpdateApiKeyRequest, ApiKeyResponse>(
            _httpClient,
            $"organizations/{organizationId}/api-keys/{apiKeyId}",
            request,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<StatusResponse> DeleteApiKeyAsync(
        OrganizationId organizationId,
        ApiKeyId apiKeyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Deleting API key {ApiKeyId} for organization {OrganizationId}",
            apiKeyId.Value,
            organizationId.Value);

        return await _executor.ExecuteDeleteAsync<StatusResponse>(
            _httpClient,
            $"organizations/{organizationId}/api-keys/{apiKeyId}",
            cancellationToken).ConfigureAwait(false);
    }
}
