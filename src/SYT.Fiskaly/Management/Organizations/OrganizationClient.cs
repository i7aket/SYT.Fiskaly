using System.Text.Json;
using SYT.Fiskaly.Authentication.ValueObjects;
using SYT.Fiskaly.Common.Enums;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.Management.Common.Responses;
using SYT.Fiskaly.Management.Organizations.Models;
using SYT.Fiskaly.Management.Organizations.Requests;
using SYT.Fiskaly.Management.Organizations.Responses;
using SYT.Fiskaly.SignDE.Common;

namespace SYT.Fiskaly.Management.Organizations;

public partial class OrganizationClient(
    HttpClient httpClient,
    FiskalyHttpRequestExecutor executor,
    ILogger<OrganizationClient> logger,
    JsonSerializerOptions serializerOptions)
    : IOrganizationClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly FiskalyHttpRequestExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    private readonly ILogger<OrganizationClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

    public async Task<ListOrganizationsResponse> ListOrganizationsAsync(
        ListOrganizationsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        string url = queryParameters?.BuildUrl("organizations") ?? "organizations";

        _logger.LogDebug("Listing organizations with URL: {Url}", url);

        ListOrganizationsResponse response = await _executor.ExecuteGetAsync<ListOrganizationsResponse>(
            _httpClient,
            url,
            cancellationToken).ConfigureAwait(false);

        int dataCount = response.Data?.Count ?? 0;
        int apiCount = response.Count ?? dataCount;
        _logger.LogInformation("Retrieved {Count} organizations (API count: {ApiCount})",
            dataCount, apiCount);

        return response;
    }

    public async Task<OrganizationResponse> GetOrganizationAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving organization: {OrganizationId}", organizationId.Value);

        OrganizationResponse organization = await _executor.ExecuteGetAsync<OrganizationResponse>(
            _httpClient,
            $"organizations/{organizationId}",
            cancellationToken).ConfigureAwait(false);

        string retrievedId = (organization.Id ?? organizationId).ToString();
        string retrievedName = organization.Name ?? "UNKNOWN";
        _logger.LogInformation("Retrieved organization: {OrganizationId}, Name: {Name}",
            retrievedId, retrievedName);

        return organization;
    }

    public async Task<OrganizationResponse> CreateOrganizationAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Creating organization {Name}", request.Name);

        OrganizationResponse organization = await _executor.ExecutePostAsync<CreateOrganizationRequest, OrganizationResponse>(
            _httpClient,
            "organizations",
            request,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Created organization {OrganizationId} with name {Name}",
            organization.Id?.Value,
            organization.Name ?? request.Name);

        return organization;
    }

    public async Task<OrganizationResponse> UpdateOrganizationAsync(
        OrganizationId organizationId,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating organization {OrganizationId}", organizationId.Value);

        OrganizationResponse organization = await _executor.ExecutePatchAsync<UpdateOrganizationRequest, OrganizationResponse>(
            _httpClient,
            $"organizations/{organizationId}",
            request,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Updated organization {OrganizationId}, Name: {Name}",
            organization.Id?.Value ?? organizationId.Value,
            organization.Name ?? "(unknown)");

        return organization;
    }

    public async Task<StatusResponse> EnableEnvironmentAsync(
        OrganizationId organizationId,
        Env env,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Enabling environment {Environment} for organization {OrganizationId}",
            env,
            organizationId.Value);

        return await _executor.ExecutePostAsync<EnableOrganizationEnvironmentRequest, StatusResponse>(
            _httpClient,
            $"organizations/{organizationId}/enable-env",
            new EnableOrganizationEnvironmentRequest { Env = env },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<StatusResponse> DeleteOrganizationAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Deleting organization {OrganizationId}", organizationId.Value);

        return await _executor.ExecuteDeleteAsync<StatusResponse>(
            _httpClient,
            $"organizations/{organizationId}",
            cancellationToken).ConfigureAwait(false);
    }
}
