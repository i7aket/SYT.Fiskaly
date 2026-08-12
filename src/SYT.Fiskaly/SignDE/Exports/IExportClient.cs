
using SYT.Fiskaly.SignDE.Exports.Models;
using SYT.Fiskaly.SignDE.Exports.Responses;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.SignDE.Exports;

/// <summary>
/// Wraps SIGN DE export endpoints (/api/v2/tss/*/export*) from the official Fiskaly SIGN DE API.
/// </summary>
public interface IExportClient
{
    /// <summary>
    /// Calls PUT /api/v2/tss/{tss_id}/export/{export_id}.
    /// </summary>
    /// <remarks>
    /// One method for one endpoint. fiskaly models the filter as a single flat querystring of nine optional
    /// parameters; the three methods that stood here until rc.8 were an SDK invention, and two of them emitted
    /// identical requests. Use <see cref="Models.ExportRequest.ForClient"/> for a client-scoped export, which
    /// is the only variant fiskaly treats specially.
    /// </remarks>
    Task<ExportJob> TriggerExportAsync(
        TssId tssId,
        ExportId exportId,
        ExportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/export/{export_id}.
    /// </summary>
    Task<ExportJob> GetExportAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/export/{export_id}/file and returns the archive verbatim.
    /// </summary>
    /// <remarks>
    /// The bytes are not parsed. fiskaly describes the payload as the TAR containing the SMAERS
    /// initialization information, the signed log messages and the certificates to verify them — TSE records,
    /// not DSFinV-K tables. A caller archiving them for the ten-year duty must keep what the provider sent.
    /// </remarks>
    Task<ExportArchive> DownloadExportAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls DELETE /api/v2/tss/{tss_id}/export/{export_id} to cancel an export job.
    /// </summary>
    Task<ExportJob> CancelExportAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/export with optional filters.
    /// </summary>
    Task<ListExportsResponse> ListExportsAsync(
        TssId tssId,
        ListExportsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/export (organization scope) and returns the paged result.
    /// </summary>
    Task<ListExportsResponse> ListAllExportsAsync(
        ListExportsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET /api/v2/tss/{tss_id}/export/{export_id}/metadata.
    /// </summary>
    Task<MetadataCollection> GetExportMetadataAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls PATCH /api/v2/tss/{tss_id}/export/{export_id}/metadata.
    /// </summary>
    Task<MetadataCollection> UpdateExportMetadataAsync(
        TssId tssId,
        ExportId exportId,
        MetadataCollection metadata,
        CancellationToken cancellationToken = default);
}
