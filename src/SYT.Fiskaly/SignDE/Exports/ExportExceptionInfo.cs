using SYT.Fiskaly.Exceptions;

namespace SYT.Fiskaly.SignDE.Exports;

public record ExportExceptionInfo(
    FiskalyErrorCategory Category,
    bool IsRetryable,
    string RecoveryHint);
