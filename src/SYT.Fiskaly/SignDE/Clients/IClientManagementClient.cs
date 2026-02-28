using SYT.Fiskaly.SignDE.Clients.Models;
using SYT.Fiskaly.SignDE.Clients.Requests;
using SYT.Fiskaly.SignDE.Clients.Responses;
using SYT.Fiskaly.SignDE.Clients.ValueObjects;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.SignDE.Clients;

/// <summary>
/// Wraps SIGN DE client endpoints (/api/v2/tss/{tss_id}/client*).
/// </summary>
public interface IClientManagementClient
{
    /// <summary>
    /// Calls PUT /api/v2/tss/{tss_id}/client/{client_id} to register a client.
    /// </summary>
    Task<ClientResponse> CreateClientAsync(
        TssId tssId,
        ClientId clientId,
        CreateClientRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls PATCH /api/v2/tss/{tss_id}/client/{client_id} to update state/metadata.
    /// </summary>
    Task<ClientResponse> UpdateClientAsync(
        TssId tssId,
        ClientId clientId,
        UpdateClientRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/client/{client_id}.
    /// </summary>
    Task<ClientResponse> GetClientAsync(
        TssId tssId,
        ClientId clientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/client with optional filters.
    /// </summary>
    Task<ClientListResponse> ListClientsAsync(
        TssId tssId,
        ListClientsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-paginates GET /api/v2/tss/{tss_id}/client to return all clients.
    /// </summary>
    Task<ClientListResponse> ListAllClientsAsync(
        ListClientsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/client/{client_id}/metadata.
    /// </summary>
    Task<MetadataCollection> GetClientMetadataAsync(
        TssId tssId,
        ClientId clientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls PATCH /api/v2/tss/{tss_id}/client/{client_id}/metadata.
    /// </summary>
    Task<MetadataCollection> UpdateClientMetadataAsync(
        TssId tssId,
        ClientId clientId,
        MetadataCollection metadata,
        CancellationToken cancellationToken = default);
}
