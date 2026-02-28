using SYT.Fiskaly.Authentication.Models;

namespace SYT.Fiskaly.Authentication.Credentials;

/// <summary>
/// Represents a credential strategy that can build the payload for POST /api/v2/auth.
/// </summary>
public interface IFiskalyCredentials
{
    /// <summary>
    /// Creates the AuthenticationPayload that the SDK posts to /api/v2/auth.
    /// </summary>
    AuthenticationPayload CreatePayload();
}
