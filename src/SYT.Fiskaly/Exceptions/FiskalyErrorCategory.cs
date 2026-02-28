#nullable enable

namespace SYT.Fiskaly.Exceptions;

public enum FiskalyErrorCategory
{
    Permanent,

    Transient,

    Infrastructure,

    Authentication
}
