using SYT.Fiskaly.SignDE.Exports;
using SYT.Fiskaly.SignDE.Exports.ValueObjects;

namespace SYT.Fiskaly.UnitTests.SignDE.Exports;

/// <summary>
/// The archive's whole job is to hand back exactly the bytes the provider sent. Everything here asserts that,
/// because the caller stores those bytes under a ten-year retention lock and a single altered byte is
/// unrecoverable.
/// </summary>
[Trait("Category", "Unit")]
public class ExportArchiveTests
{
    private static readonly ExportId AnyExportId = ExportId.From("11111111-2222-4333-8444-555555555555");

    [Fact]
    public async Task FromStreamAsync_KeepsEveryByte()
    {
        byte[] payload = [.. Enumerable.Range(0, 5000).Select(i => (byte)(i % 251))];
        using MemoryStream source = new(payload, writable: false);

        ExportArchive archive = await ExportArchive.FromStreamAsync(AnyExportId, source);

        Assert.Equal(payload, archive.RawContent.ToArray());
        Assert.Equal(AnyExportId, archive.ExportId);
    }

    /// <summary>
    /// The response body from HttpClient is forward-only. Reading it must not depend on seeking.
    /// </summary>
    [Fact]
    public async Task FromStreamAsync_ReadsANonSeekableStream()
    {
        byte[] payload = [.. Enumerable.Range(0, 3000).Select(i => (byte)i)];
        using ForwardOnlyStream source = new(payload);

        ExportArchive archive = await ExportArchive.FromStreamAsync(AnyExportId, source);

        Assert.Equal(payload, archive.RawContent.ToArray());
    }

    [Fact]
    public async Task FromStreamAsync_NullStream_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => ExportArchive.FromStreamAsync(AnyExportId, null!));
    }

    [Fact]
    public async Task FromStreamAsync_EmptyStream_YieldsEmptyContent()
    {
        using MemoryStream source = new([], writable: false);

        ExportArchive archive = await ExportArchive.FromStreamAsync(AnyExportId, source);

        Assert.Empty(archive.RawContent.ToArray());
    }

    [Fact]
    public async Task OpenRawStream_HandsOutIndependentReaders()
    {
        byte[] payload = [1, 2, 3, 4, 5];
        using MemoryStream source = new(payload, writable: false);
        ExportArchive archive = await ExportArchive.FromStreamAsync(AnyExportId, source);

        using Stream first = archive.OpenRawStream();
        using Stream second = archive.OpenRawStream();
        first.ReadByte();

        Assert.Equal(1, first.Position);
        Assert.Equal(0, second.Position);
        Assert.Equal(payload, ReadAll(second));
    }

    /// <summary>
    /// rc.8 removed the segment model. This asserts the type no longer parses what it is handed: a payload
    /// that is not an archive at all still round-trips, because interpreting the bytes was never this type's
    /// job and a parse would only reject data the provider legitimately sent.
    /// </summary>
    [Fact]
    public async Task FromStreamAsync_DoesNotParse_SoNonArchiveContentSurvives()
    {
        byte[] notAnArchive = "this is not a tar file"u8.ToArray();
        using MemoryStream source = new(notAnArchive, writable: false);

        ExportArchive archive = await ExportArchive.FromStreamAsync(AnyExportId, source);

        Assert.Equal(notAnArchive, archive.RawContent.ToArray());
    }

    private static byte[] ReadAll(Stream stream)
    {
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private sealed class ForwardOnlyStream(byte[] payload) : Stream
    {
        private readonly MemoryStream _inner = new(payload, writable: false);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _inner.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
