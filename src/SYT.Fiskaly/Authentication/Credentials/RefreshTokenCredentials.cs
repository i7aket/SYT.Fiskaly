using SYT.Fiskaly.Authentication.Models;
using SYT.Fiskaly.Authentication.ValueObjects;

namespace SYT.Fiskaly.Authentication.Credentials;

public sealed class RefreshTokenCredentials : IFiskalyCredentials
{
    public RefreshTokenCredentials(RefreshToken refreshToken)
    {
        RefreshToken = refreshToken;
    }

    public RefreshToken RefreshToken { get; }

    public AuthenticationPayload CreatePayload() => new RefreshTokenAuthenticationPayload(RefreshToken);
}
