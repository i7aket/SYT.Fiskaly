using System.Text.Json;
using SYT.Fiskaly.Exceptions;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.SignDE.Exports.Enums;
using SYT.Fiskaly.SignDE.Exports.Models;
using SYT.Fiskaly.SignDE.Exports.Responses;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.SignDE.Exports;

public class ExportClient(
    HttpClient httpClient,
    FiskalyHttpRequestExecutor executor,
    ILogger<ExportClient> logger,
    JsonSerializerOptions serializerOptions)
    : IExportClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly FiskalyHttpRequestExecutor _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    private readonly ILogger<ExportClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

    public async Task<ExportJob> TriggerExportAsync(
        TssId tssId,
        ExportId exportId,
        ExportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // One method, because fiskaly has one endpoint and one query shape. The three that stood here until
        // rc.8 differed only in their request type and a log string, and two of them built byte-identical
        // query strings whenever only a counter range was set.
        _logger.LogInformation(
            "Triggering export {ExportId} for TSS {TssId} (Client: {ClientId}, StartDate: {StartDate}, "
            + "EndDate: {EndDate}, SignatureCounters: {StartSignatureCounter}..{EndSignatureCounter})",
            exportId.Value,
            tssId.Value,
            request.ClientId?.ToString() ?? "all",
            request.StartDate,
            request.EndDate,
            request.StartSignatureCounter?.Value,
            request.EndSignatureCounter?.Value);

        string url = request.BuildUrl($"tss/{tssId}/export/{exportId}");

        ExportJob exportResponse = await _executor.ExecutePutAsync<ExportJob>(_httpClient, url, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Export triggered: {ExportId}, State: {State}", exportResponse.Id.Value, exportResponse.State);

        return exportResponse;
    }

    public async Task<ExportJob> GetExportAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching export {ExportId} status for TSS {TssId}", exportId.Value, tssId.Value);

        ExportJob exportResponse = await _executor.ExecuteGetAsync<ExportJob>(_httpClient, $"tss/{tssId.Value}/export/{exportId.Value}", cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Export {ExportId} state: {State}", exportId.Value, exportResponse.State);

        return exportResponse;
    }

    public async Task<ExportArchive> DownloadExportAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading export {ExportId} for TSS {TssId}", exportId.Value, tssId.Value);

        ExportJob export = await GetExportAsync(tssId, exportId, cancellationToken).ConfigureAwait(false);

        if (export.State != ExportState.Completed)
        {
            // A typed exception inside the Fiskaly hierarchy, not a bare InvalidOperationException: this is an
            // ordinary race - the state is read here, immediately before the download - and callers catch
            // FiskalyException. Left outside the hierarchy it escaped as an unhandled exception and reached
            // consuming applications as a 500.
            FiskalyExportNotReadyException notReady = new(exportId, export.State, export.ExceptionCode);
            _logger.LogWarning("{Message}", notReady.Message);
            throw notReady;
        }

        // No EnsureSuccessStatusCode here on purpose: FiskalyErrorHandler sits in every typed client's pipeline
        // (AddFiskalyPipeline) and has already turned any non-2xx into a FiskalyApiException before the
        // response gets back here, so the check would be dead code that hides where the real guard lives.
        using HttpResponseMessage response = await _httpClient.GetAsync($"tss/{tssId.Value}/export/{exportId.Value}/file", cancellationToken).ConfigureAwait(false);
        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        // The bytes are kept as sent. Parsing them here would buy nothing: the caller archiving a journal for
        // ten years has to store what the provider sent, and .NET's TarReader treats a truncated archive as a
        // complete one anyway, so a parse would not even prove the download finished.
        return await ExportArchive.FromStreamAsync(exportId, stream, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ExportJob> CancelExportAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling export {ExportId} for TSS {TssId}", exportId.Value, tssId.Value);

        ExportJob exportResponse = await _executor.ExecuteDeleteAsync<ExportJob>(_httpClient, $"tss/{tssId.Value}/export/{exportId.Value}", cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Export cancelled: {ExportId}, State: {State}", exportResponse.Id.Value, exportResponse.State);

        return exportResponse;
    }

    public async Task<ListExportsResponse> ListExportsAsync(
        TssId tssId,
        ListExportsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        string url = queryParameters?.BuildUrl($"tss/{tssId.Value}/export") ?? $"tss/{tssId.Value}/export";

        ListExportsResponse response = await _executor.ExecuteGetAsync<ListExportsResponse>(_httpClient, url, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved {Count} exports for TSS {TssId}",
            response.Data.Count, tssId.Value);

        return response;
    }

    public async Task<ListExportsResponse> ListAllExportsAsync(
        ListExportsQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        string url = queryParameters?.BuildUrl("export") ?? "export";

        ListExportsResponse response = await _executor.ExecuteGetAsync<ListExportsResponse>(_httpClient, url, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved {Count} exports across all TSS",
            response.Data.Count);

        return response;
    }

    public async Task<MetadataCollection> GetExportMetadataAsync(
        TssId tssId,
        ExportId exportId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting metadata for export {ExportId} (TSS: {TssId})",
            exportId.Value, tssId.Value);

        MetadataCollection metadata = await MetadataOperations.GetAsync(
            _executor,
            _httpClient,
            $"tss/{tssId.Value}/export/{exportId.Value}/metadata",
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved metadata for export {ExportId} with {Count} entries",
            exportId.Value, metadata.Count);

        return metadata;
    }

    public async Task<MetadataCollection> UpdateExportMetadataAsync(
        TssId tssId,
        ExportId exportId,
        MetadataCollection metadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

        _logger.LogInformation("Updating export metadata for {ExportId} (TSS: {TssId}) with {Count} entries",
            exportId.Value, tssId.Value, metadata.Count);

        MetadataCollection result = await MetadataOperations.UpdateAsync(
            _executor,
            _httpClient,
            $"tss/{tssId.Value}/export/{exportId.Value}/metadata",
            metadata,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Updated export metadata for {ExportId}, new count: {Count}",
            exportId.Value, result.Count);

        return result;
    }
}
