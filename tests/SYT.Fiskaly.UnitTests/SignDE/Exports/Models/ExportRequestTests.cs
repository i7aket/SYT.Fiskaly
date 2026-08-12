using SYT.Fiskaly.Exceptions;
using SYT.Fiskaly.SignDE.Clients.ValueObjects;
using SYT.Fiskaly.SignDE.Exports.Models;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;

namespace SYT.Fiskaly.UnitTests.SignDE.Exports.Models;

/// <summary>
/// Replaces the three per-variant request suites rc.8 collapsed. Every assertion here exists because getting
/// it wrong produces a request fiskaly accepts and answers with the wrong data — none of these failures is
/// loud on its own.
/// </summary>
[Trait("Category", "Unit")]
public class ExportRequestTests
{
    private static readonly ClientId AnyClient = ClientId.From("aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeeeeeee");

    private static Dictionary<string, string?> Query(ExportRequest request) =>
        request.ToQueryParameters().ToDictionary(p => p.Key, p => p.Value);

    [Fact]
    public void EmptyRequest_EmitsNothing()
    {
        Assert.Empty(Query(new ExportRequest()));
    }

    /// <summary>
    /// Every parameter must carry fiskaly's exact key. A typo drops the filter silently and exports a
    /// different range than the caller asked for.
    /// </summary>
    [Fact]
    public void EveryParameter_UsesTheProvidersKey()
    {
        ExportRequest request = new()
        {
            TransactionNumber = TransactionSequenceNumber.From(7),
            StartTransactionNumber = TransactionSequenceNumber.From(1),
            EndTransactionNumber = TransactionSequenceNumber.From(9),
            StartDate = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000),
            EndDate = DateTimeOffset.FromUnixTimeSeconds(1_700_003_600),
            MaximumNumberRecords = ExportLimit.From(500),
            StartSignatureCounter = SignatureCounter.From(10),
            EndSignatureCounter = SignatureCounter.From(20)
        };

        Dictionary<string, string?> query = Query(request);

        Assert.Equal("7", query["transaction_number"]);
        Assert.Equal("1", query["start_transaction_number"]);
        Assert.Equal("9", query["end_transaction_number"]);
        Assert.Equal("1700000000", query["start_date"]);
        Assert.Equal("1700003600", query["end_date"]);
        Assert.Equal("500", query["maximum_number_records"]);
        Assert.Equal("10", query["start_signature_counter"]);
        Assert.Equal("20", query["end_signature_counter"]);
    }

    /// <summary>
    /// Unix seconds, not ISO-8601. The format is invisible to the compiler and a "tidy-up" that switches to
    /// ISO would be accepted by nobody and noticed by no test but this one.
    /// </summary>
    [Fact]
    public void Dates_SerializeAsUnixSeconds()
    {
        ExportRequest request = new()
        {
            StartDate = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal("1780315200", Query(request)["start_date"]);
    }

    [Fact]
    public void UnsetParameters_EmitNoKeyAtAll()
    {
        ExportRequest request = new() { StartSignatureCounter = SignatureCounter.From(3) };

        Dictionary<string, string?> query = Query(request);

        Assert.True(query.ContainsKey("start_signature_counter"));
        Assert.False(query.ContainsKey("end_signature_counter"));
        Assert.False(query.ContainsKey("client_id"));
        Assert.False(query.ContainsKey("start_date"));
    }

    // --- the three validations that existed before the collapse, and must survive it ---

    [Fact]
    public void ReversedSignatureCounterRange_IsRefused()
    {
        ExportRequest request = new()
        {
            StartSignatureCounter = SignatureCounter.From(50),
            EndSignatureCounter = SignatureCounter.From(10)
        };

        FiskalyValidationException ex = Assert.Throws<FiskalyValidationException>(() => Query(request));
        Assert.Contains("signature counter", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReversedTransactionNumberRange_IsRefused()
    {
        ExportRequest request = new()
        {
            StartTransactionNumber = TransactionSequenceNumber.From(50),
            EndTransactionNumber = TransactionSequenceNumber.From(10)
        };

        Assert.Throws<FiskalyValidationException>(() => Query(request));
    }

    [Fact]
    public void EndDateBeforeStartDate_IsRefused()
    {
        ExportRequest request = new()
        {
            StartDate = DateTimeOffset.FromUnixTimeSeconds(2000),
            EndDate = DateTimeOffset.FromUnixTimeSeconds(1000)
        };

        Assert.Throws<FiskalyValidationException>(() => Query(request));
    }

    /// <summary>
    /// The spec types both dates as <c>integer, minimum: 0</c>. A pre-epoch DateTimeOffset serialises to a
    /// negative number, which is out of range — and the value the caller would have to debug is one fiskaly
    /// never echoes back.
    /// </summary>
    [Fact]
    public void DateBeforeTheUnixEpoch_IsRefused()
    {
        ExportRequest start = new() { StartDate = new DateTimeOffset(1969, 12, 31, 0, 0, 0, TimeSpan.Zero) };
        ExportRequest end = new() { EndDate = new DateTimeOffset(1969, 12, 31, 0, 0, 0, TimeSpan.Zero) };

        Assert.Throws<FiskalyValidationException>(() => Query(start));
        Assert.Throws<FiskalyValidationException>(() => Query(end));
    }

    [Fact]
    public void TheEpochItself_IsAccepted()
    {
        Dictionary<string, string?> query = Query(new ExportRequest { StartDate = DateTimeOffset.UnixEpoch });

        Assert.Equal("0", query["start_date"]);
    }

    // --- client_id exclusivity: the defect this collapse exists to close ---

    [Fact]
    public void ClientId_Alone_IsAccepted()
    {
        Dictionary<string, string?> query = Query(new ExportRequest { ClientId = AnyClient });

        Assert.Equal(AnyClient.ToString(), query["client_id"]);
        Assert.Single(query);
    }

    [Theory]
    [MemberData(nameof(EveryOtherFilter))]
    public void ClientId_WithAnyOtherFilter_IsRefused(string label, ExportRequest request)
    {
        Assert.NotNull(label);

        FiskalyValidationException ex = Assert.Throws<FiskalyValidationException>(() => Query(request));
        Assert.Contains("client_id", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ForClient", ex.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// All eight, including <c>MaximumNumberRecords</c> — it reads like a cap rather than a filter, which is
    /// exactly why it is easy to assume fiskaly still honours it alongside a client.
    /// </summary>
    public static TheoryData<string, ExportRequest> EveryOtherFilter() => new()
    {
        { "transaction_number", new ExportRequest { ClientId = AnyClient, TransactionNumber = TransactionSequenceNumber.From(1) } },
        { "start_transaction_number", new ExportRequest { ClientId = AnyClient, StartTransactionNumber = TransactionSequenceNumber.From(1) } },
        { "end_transaction_number", new ExportRequest { ClientId = AnyClient, EndTransactionNumber = TransactionSequenceNumber.From(1) } },
        { "start_date", new ExportRequest { ClientId = AnyClient, StartDate = DateTimeOffset.UnixEpoch } },
        { "end_date", new ExportRequest { ClientId = AnyClient, EndDate = DateTimeOffset.UnixEpoch } },
        { "maximum_number_records", new ExportRequest { ClientId = AnyClient, MaximumNumberRecords = ExportLimit.From(10) } },
        { "start_signature_counter", new ExportRequest { ClientId = AnyClient, StartSignatureCounter = SignatureCounter.From(1) } },
        { "end_signature_counter", new ExportRequest { ClientId = AnyClient, EndSignatureCounter = SignatureCounter.From(1) } }
    };

    /// <summary>
    /// The factory is the reason the collapse does not lose the guarantee the old client-only class provided:
    /// what it builds cannot violate the rule.
    /// </summary>
    [Fact]
    public void ForClient_SetsTheClientAndNothingElse()
    {
        Dictionary<string, string?> query = Query(ExportRequest.ForClient(AnyClient));

        Assert.Single(query);
        Assert.Equal(AnyClient.ToString(), query["client_id"]);
    }

    /// <summary>
    /// Inside the FiskalyException hierarchy on purpose — a consumer catching FiskalyException answers 4xx,
    /// while a bare InvalidOperationException escapes as a 500 (the bug rc.7 fixed for downloads).
    /// </summary>
    [Fact]
    public void ValidationFailures_AreFiskalyExceptions()
    {
        ExportRequest request = new()
        {
            ClientId = AnyClient,
            StartDate = DateTimeOffset.UnixEpoch
        };

        Assert.IsAssignableFrom<FiskalyException>(
            Assert.ThrowsAny<Exception>(() => Query(request)));
    }
}
