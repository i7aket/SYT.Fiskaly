using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using SYT.Fiskaly.Common.Enums;
using SYT.Fiskaly.Exceptions;
using SYT.Fiskaly.Http;
using SYT.Fiskaly.SignDE.Common;
using SYT.Fiskaly.SignDE.Exports;
using SYT.Fiskaly.SignDE.Exports.Dsfinvk;
using SYT.Fiskaly.SignDE.Exports.Enums;
using SYT.Fiskaly.SignDE.Exports.Models;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;
using SYT.Fiskaly.SignDE.Tss.ValueObjects;

namespace SYT.Fiskaly.UnitTests.SignDE.Exports;

/// <summary>
/// DownloadExportAsync reads the export's state and refuses anything that is not COMPLETED. What it throws
/// matters as much as that it throws: the refusal used to be a bare InvalidOperationException, outside the
/// FiskalyException hierarchy every caller catches, so an ordinary race - poll says WORKING, the download a
/// moment later - escaped as an unhandled exception and surfaced as a 500 in the consuming application.
/// </summary>
[Trait("Category", "Unit")]
public class ExportClientDownloadStateTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new MetadataCollectionJsonConverter() }
    };

    private readonly TssId _tss = TssId.New();
    private readonly ExportId _export = ExportId.New();

    [Theory]
    [InlineData(ExportState.Pending)]
    [InlineData(ExportState.Working)]
    public async Task DownloadExport_BeforeItFinishes_ThrowsATypedTransientFailure(ExportState state)
    {
        ExportClient sut = CreateClient(state);

        FiskalyExportNotReadyException thrown = await Assert.ThrowsAsync<FiskalyExportNotReadyException>(
            () => sut.DownloadExportAsync(_tss, _export));

        Assert.IsAssignableFrom<FiskalyException>(thrown);
        Assert.Equal(state, thrown.State);
        Assert.Equal(_export, thrown.ExportId);
        Assert.True(thrown.IsTransient, "waiting and asking again is the correct response to this one");
    }

    /// <summary>
    /// A failed export is the same refusal with the opposite advice: it will never become downloadable, so a
    /// caller that keeps polling waits forever. That is why the state travels on the exception.
    /// </summary>
    [Fact]
    public async Task DownloadExport_WhenTheExportFailed_ThrowsATerminalFailureNamingTheCause()
    {
        ExportClient sut = CreateClient(ExportState.Error, ExportExceptionCode.Internal);

        FiskalyExportNotReadyException thrown = await Assert.ThrowsAsync<FiskalyExportNotReadyException>(
            () => sut.DownloadExportAsync(_tss, _export));

        Assert.Equal(ExportState.Error, thrown.State);
        Assert.False(thrown.IsTransient);
        Assert.Equal(ExportExceptionCode.Internal, thrown.ExceptionCode);
        Assert.Contains("Trigger a new export", thrown.Message, StringComparison.Ordinal);
    }

    private ExportClient CreateClient(ExportState state, ExportExceptionCode? exceptionCode = null)
    {
        ExportJob job = new()
        {
            Id = _export,
            TssId = _tss,
            State = state,
            ExceptionCode = exceptionCode,
            Env = Env.Test,
            Type = ResourceType.Export,
        };

        Mock<HttpMessageHandler> handler = new(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(job, _jsonOptions), Encoding.UTF8, "application/json")
            });

        HttpClient httpClient = new(handler.Object)
        {
            BaseAddress = new Uri("https://kassensichv-middleware.fiskaly.com/api/v2/")
        };

        return new ExportClient(
            httpClient,
            new FiskalyHttpRequestExecutor(_jsonOptions, NullLogger<FiskalyHttpRequestExecutor>.Instance),
            NullLogger<ExportClient>.Instance,
            _jsonOptions,
            new DsfinvkV2SegmentStrategy());
    }
}
