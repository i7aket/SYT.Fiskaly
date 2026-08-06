using SYT.Fiskaly.Authentication.Credentials;
using SYT.Fiskaly.Authentication.Models;

namespace SYT.Fiskaly.Authentication;

/// <summary>
/// Issues Fiskaly JWT tokens via POST /api/v2/auth.
/// </summary>
public interface IFiskalyAuthenticationService : IDisposable
{
    /// <summary>
    /// Returns a cached or freshly requested JWT for POST /api/v2/auth.
    /// </summary>
    /// <param name="cancellationToken">Forwarded to the underlying HTTP call.</param>
    /// <returns>Bearer token text for the Authorization header.</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a JWT for the supplied credential strategy via POST /api/v2/auth.
    /// </summary>
    /// <param name="credentials">Strategy (API key, refresh token, service account).</param>
    /// <param name="cancellationToken">Forwarded to the HTTP call.</param>
    /// <returns>Bearer token text for the Authorization header.</returns>
    Task<string> GetAccessTokenAsync(IFiskalyCredentials credentials, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes POST /api/v2/auth and returns the entire AuthenticationResponse payload.
    /// </summary>
    /// <param name="credentials">Strategy (API key, refresh token, service account).</param>
    /// <param name="cancellationToken">Forwarded to the HTTP call.</param>
    /// <returns>The parsed AuthenticationResponse from fiskaly.</returns>
    Task<AuthenticationResponse> AuthenticateAsync(IFiskalyCredentials credentials, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops the cached token for these credentials, so the next call authenticates afresh.
    ///
    /// <para>A token can stop being accepted long before it expires - fiskaly invalidate sessions on their side,
    /// and a key deletion leaves issued tokens alive for up to 24 hours by their own documentation, which says
    /// the reverse happens too. Without this the cache would keep handing out the rejected token until its
    /// nominal expiry, and every retry would resend exactly what was just refused.</para>
    /// </summary>
    /// <param name="credentials">Whose token to drop; the configured default when null.</param>
    void InvalidateToken(IFiskalyCredentials? credentials = null);
}
