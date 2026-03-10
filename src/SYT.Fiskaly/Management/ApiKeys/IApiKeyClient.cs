using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Management.ApiKeys.Models;
using SYT.Fiskaly.Management.ApiKeys.Requests;
using SYT.Fiskaly.Management.ApiKeys.Responses;
using SYT.Fiskaly.Management.Common.Responses;

namespace SYT.Fiskaly.Management.ApiKeys;

/// <summary>
/// Wrapper for Management API API key endpoints scoped to an organization.
/// </summary>
public interface IApiKeyClient
{
    /// <summary>
    /// Calls GET /organizations/{organization_id}/api-keys with optional filters, paging, and sorting.
    /// </summary>
    /// <param name="organizationId">Owning organization identifier.</param>
    /// <param name="queryParameters">Optional query model for filtering, sorting, and paging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API key list response from the Management API.</returns>
    Task<ListApiKeyResponse> ListApiKeysAsync(
        OrganizationId organizationId,
        ListApiKeysQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /organizations/{organization_id}/api-keys/{api_key_id}.
    /// </summary>
    /// <param name="organizationId">Owning organization identifier.</param>
    /// <param name="apiKeyId">API key identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API key resource from the Management API.</returns>
    Task<ApiKeyResponse> GetApiKeyAsync(
        OrganizationId organizationId,
        ApiKeyId apiKeyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls POST /organizations/{organization_id}/api-keys.
    /// </summary>
    /// <param name="organizationId">Owning organization identifier.</param>
    /// <param name="request">Create request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created API key resource, including one-time secret material.</returns>
    Task<ApiKeyResponse> CreateApiKeyAsync(
        OrganizationId organizationId,
        CreateApiKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls PATCH /organizations/{organization_id}/api-keys/{api_key_id}.
    /// </summary>
    /// <param name="organizationId">Owning organization identifier.</param>
    /// <param name="apiKeyId">API key identifier.</param>
    /// <param name="request">Update request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated API key resource.</returns>
    Task<ApiKeyResponse> UpdateApiKeyAsync(
        OrganizationId organizationId,
        ApiKeyId apiKeyId,
        UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls DELETE /organizations/{organization_id}/api-keys/{api_key_id}.
    /// </summary>
    /// <param name="organizationId">Owning organization identifier.</param>
    /// <param name="apiKeyId">API key identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status response from the Management API.</returns>
    Task<StatusResponse> DeleteApiKeyAsync(
        OrganizationId organizationId,
        ApiKeyId apiKeyId,
        CancellationToken cancellationToken = default);
}
