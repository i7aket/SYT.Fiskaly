using SYT.Fiskaly.SignDE.Exports.Enums;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;

namespace SYT.Fiskaly.Exceptions;

/// <summary>
/// Thrown when an export archive is requested before the export can be downloaded.
/// </summary>
/// <remarks>
/// Two situations reach this exception and callers must treat them differently, which is why
/// <see cref="IsTransient"/> exists rather than leaving the caller to compare <see cref="State"/> itself:
/// a PENDING or WORKING export becomes downloadable on its own, so the right response is to keep polling;
/// an ERROR one never will, so polling it is an infinite wait.
/// <para>
/// It derives from <see cref="FiskalyException"/> deliberately. Before this type existed the same condition
/// was a bare <c>InvalidOperationException</c>, which sits outside the hierarchy every caller catches - so a
/// perfectly ordinary race (poll says WORKING, download a moment later) escaped as an unhandled exception and
/// surfaced as a 500 in the consuming application.
/// </para>
/// </remarks>
public class FiskalyExportNotReadyException : FiskalyException
{
    /// <summary>The export's state at the provider when the download was refused.</summary>
    public ExportState State { get; }

    /// <summary>The export id that was requested.</summary>
    public ExportId ExportId { get; }

    /// <summary>Provider exception code, when the export carries one (present for <see cref="ExportState.Error"/>).</summary>
    public ExportExceptionCode? ExceptionCode { get; }

    /// <summary>
    /// True when waiting and asking again is the correct response - the export has not finished yet. False for
    /// a terminal state, where the only way forward is to trigger a new export.
    /// </summary>
    public bool IsTransient => State is ExportState.Pending or ExportState.Working;

    public FiskalyExportNotReadyException(
        ExportId exportId,
        ExportState state,
        ExportExceptionCode? exceptionCode = null)
        : base(BuildMessage(exportId, state, exceptionCode))
    {
        ExportId = exportId;
        State = state;
        ExceptionCode = exceptionCode;
    }

    private static string BuildMessage(ExportId exportId, ExportState state, ExportExceptionCode? exceptionCode)
        => state == ExportState.Error
            ? $"Cannot download export {exportId.Value}: the export failed at the provider (state ERROR, exception {exceptionCode?.ToString() ?? "unspecified"}). Trigger a new export."
            : $"Cannot download export {exportId.Value}: state is {state}, expected COMPLETED.";
}
