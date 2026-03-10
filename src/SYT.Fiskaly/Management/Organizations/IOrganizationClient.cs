using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Common.Enums;
using SYT.Fiskaly.Management.Common.Responses;
using SYT.Fiskaly.Management.Organizations.Models;
using SYT.Fiskaly.Management.Organizations.Requests;
using SYT.Fiskaly.Management.Organizations.Responses;

namespace SYT.Fiskaly.Management.Organizations;

/// <summary>
/// Wrapper for Management API organization endpoints (fiskaly-management-api-spec-v0.12.0).
/// Supports read and write operations for organization lifecycle management.
/// </summary>
public interface IOrganizationClient
{
    /// <summary>
    /// Calls GET /organizations with optional filters/paging.
    /// </summary>
    /// <param name="queryParameters">Filter/paging options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List response defined by the Management API spec.</returns>
    Task<ListOrganizationsResponse> ListOrganizationsAsync(
        ListOrganizationsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /organizations/{organization_id}.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OrganizationResponse from the Management API.</returns>
    Task<OrganizationResponse> GetOrganizationAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls POST /organizations.
    /// </summary>
    Task<OrganizationResponse> CreateOrganizationAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls PATCH /organizations/{organization_id}.
    /// </summary>
    Task<OrganizationResponse> UpdateOrganizationAsync(
        OrganizationId organizationId,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls POST /organizations/{organization_id}/enable-env.
    /// </summary>
    Task<StatusResponse> EnableEnvironmentAsync(
        OrganizationId organizationId,
        Env env,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls DELETE /organizations/{organization_id}.
    /// </summary>
    Task<StatusResponse> DeleteOrganizationAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);
}
