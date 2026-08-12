namespace SYT.Fiskaly.Exceptions;

/// <summary>
/// A request this SDK refuses to send, because the provider documents that it would not mean what the caller
/// asked for.
/// </summary>
/// <remarks>
/// <para>
/// Inside the <see cref="FiskalyException"/> hierarchy on purpose. A consuming application catches
/// <c>FiskalyException</c> and answers 4xx; a bare <see cref="InvalidOperationException"/> escapes that catch
/// and surfaces as an HTTP 500, which says "we broke" about a request the caller could fix. rc.7 made exactly
/// this change for <c>DownloadExportAsync</c>; rc.8 finishes the job for request validation.
/// </para>
/// <para>
/// The message names the offending parameters and the way to ask correctly, because it is going to be read by
/// whoever sent the request, not by whoever wrote this SDK.
/// </para>
/// </remarks>
public class FiskalyValidationException : FiskalyException
{
    public FiskalyValidationException(string message)
        : base(message)
    {
    }

    public FiskalyValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
