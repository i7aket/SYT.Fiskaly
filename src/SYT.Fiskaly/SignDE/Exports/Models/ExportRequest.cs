using SYT.Fiskaly.Exceptions;
using SYT.Fiskaly.SignDE.Clients.ValueObjects;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;

namespace SYT.Fiskaly.SignDE.Exports.Models;

public abstract class ExportRequestBase : IQueryParameterProvider
{
    public IEnumerable<KeyValuePair<string, string?>> ToQueryParameters()
    {
        List<KeyValuePair<string, string?>> parameters = new List<KeyValuePair<string, string?>>();
        Apply(parameters);
        return parameters;
    }

    protected abstract void Apply(List<KeyValuePair<string, string?>> parameters);

    protected static void AddParameter(List<KeyValuePair<string, string?>> parameters, string name, string? value)
    {
        if (value is null)
        {
            return;
        }

        parameters.Add(new KeyValuePair<string, string?>(name, value));
    }
}

/// <summary>
/// The filter for <c>PUT /api/v2/tss/{tss_id}/export/{export_id}</c>.
/// </summary>
/// <remarks>
/// <para>
/// One class, because fiskaly models one endpoint with one flat set of nine optional query parameters
/// (<c>TriggerExportsQuerystring</c>) — no discriminator, no variants, nothing required. Until rc.8 the SDK
/// split this into three classes named after DSFinV-K, a taxonomy the provider does not have. Two of the three
/// produced byte-identical query strings whenever only a counter range was set, so the split did not even
/// discriminate.
/// </para>
/// <para>
/// The one distinction fiskaly DOES make is that <c>client_id</c> is exclusive: <i>"Only return log messages
/// associated with the given client (other query parameters will be ignored)"</i>. Combining it used to be
/// representable and silently returned a different export than the caller asked for. It is now refused —
/// see <see cref="ForClient"/> for the safe way to ask for one client.
/// </para>
/// </remarks>
public sealed class ExportRequest : ExportRequestBase
{
    /// <summary>
    /// Exclusive: fiskaly ignores every other filter when this is set, so setting any alongside it is refused.
    /// </summary>
    public ClientId? ClientId { get; init; }

    public TransactionSequenceNumber? TransactionNumber { get; init; }

    public TransactionSequenceNumber? StartTransactionNumber { get; init; }

    public TransactionSequenceNumber? EndTransactionNumber { get; init; }

    public DateTimeOffset? StartDate { get; init; }

    public DateTimeOffset? EndDate { get; init; }

    public ExportLimit? MaximumNumberRecords { get; init; }

    public SignatureCounter? StartSignatureCounter { get; init; }

    public SignatureCounter? EndSignatureCounter { get; init; }

    /// <summary>
    /// A client-scoped export, which is the only shape fiskaly treats as exclusive. Using this instead of the
    /// initialiser makes the exclusivity rule unbreakable rather than merely checked.
    /// </summary>
    public static ExportRequest ForClient(ClientId clientId) => new() { ClientId = clientId };

    protected override void Apply(List<KeyValuePair<string, string?>> parameters)
    {
        Validate();

        if (ClientId.HasValue)
        {
            // Exclusive by the provider's own rule; nothing else is emitted, so nothing else can be ignored
            // behind the caller's back.
            AddParameter(parameters, "client_id", ClientId.Value.ToString());
            return;
        }

        if (TransactionNumber.HasValue)
        {
            AddParameter(parameters, "transaction_number", TransactionNumber.Value.Value.ToString());
        }

        if (StartTransactionNumber.HasValue)
        {
            AddParameter(parameters, "start_transaction_number", StartTransactionNumber.Value.Value.ToString());
        }

        if (EndTransactionNumber.HasValue)
        {
            AddParameter(parameters, "end_transaction_number", EndTransactionNumber.Value.Value.ToString());
        }

        if (StartDate.HasValue)
        {
            AddParameter(parameters, "start_date", StartDate.Value.ToUnixTimeSeconds().ToString());
        }

        if (EndDate.HasValue)
        {
            AddParameter(parameters, "end_date", EndDate.Value.ToUnixTimeSeconds().ToString());
        }

        if (MaximumNumberRecords.HasValue)
        {
            AddParameter(parameters, "maximum_number_records", MaximumNumberRecords.Value.Value.ToString());
        }

        if (StartSignatureCounter.HasValue)
        {
            AddParameter(parameters, "start_signature_counter", StartSignatureCounter.Value.Value.ToString());
        }

        if (EndSignatureCounter.HasValue)
        {
            AddParameter(parameters, "end_signature_counter", EndSignatureCounter.Value.Value.ToString());
        }
    }

    /// <remarks>
    /// <see cref="FiskalyValidationException"/> rather than <see cref="InvalidOperationException"/>, and
    /// deliberately: a consumer catching <c>FiskalyException</c> — which is the documented way to consume this
    /// SDK — turns this into a 4xx, while a bare framework exception escapes as a 500. rc.7 fixed exactly that
    /// shape for <c>DownloadExportAsync</c>; these throws are the same case.
    /// </remarks>
    private void Validate()
    {
        if (ClientId.HasValue && HasAnyOtherFilter())
        {
            throw new FiskalyValidationException(
                "client_id is exclusive: fiskaly ignores every other query parameter when it is set. "
                + "Ask for a client-scoped export with ExportRequest.ForClient(clientId), or drop client_id "
                + "and filter by date, transaction number or signature counter.");
        }

        if (StartSignatureCounter.HasValue
            && EndSignatureCounter.HasValue
            && EndSignatureCounter.Value.Value < StartSignatureCounter.Value.Value)
        {
            throw new FiskalyValidationException(
                "End signature counter cannot be less than start signature counter.");
        }

        if (StartTransactionNumber.HasValue
            && EndTransactionNumber.HasValue
            && EndTransactionNumber.Value.Value < StartTransactionNumber.Value.Value)
        {
            throw new FiskalyValidationException(
                "End transaction number cannot be less than start transaction number.");
        }

        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            throw new FiskalyValidationException("End date cannot be earlier than start date.");
        }

        // The spec types both dates as "integer, minimum: 0" - Unix seconds. A DateTimeOffset before the epoch
        // serialises to a negative number, which is out of range; refusing here names the parameter instead of
        // letting fiskaly answer with a generic 400 about a value it will not echo back.
        if (StartDate.HasValue && StartDate.Value.ToUnixTimeSeconds() < 0)
        {
            throw new FiskalyValidationException("Start date cannot be earlier than 1970-01-01 UTC.");
        }

        if (EndDate.HasValue && EndDate.Value.ToUnixTimeSeconds() < 0)
        {
            throw new FiskalyValidationException("End date cannot be earlier than 1970-01-01 UTC.");
        }
    }

    /// <summary>
    /// Every parameter other than <see cref="ClientId"/> — including <see cref="MaximumNumberRecords"/>, which
    /// reads like a cap but is a query parameter in fiskaly's model and is ignored alongside the rest.
    /// </summary>
    private bool HasAnyOtherFilter() =>
        TransactionNumber.HasValue
        || StartTransactionNumber.HasValue
        || EndTransactionNumber.HasValue
        || StartDate.HasValue
        || EndDate.HasValue
        || MaximumNumberRecords.HasValue
        || StartSignatureCounter.HasValue
        || EndSignatureCounter.HasValue;
}
