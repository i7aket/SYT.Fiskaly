namespace SYT.Fiskaly.SignDE.Exports.Dsfinvk;

public sealed class UnknownDsfinvkSegment(string fileName, byte[] content)
    : DsfinvkSegment(DsfinvkSegmentType.Unknown, fileName, content);
