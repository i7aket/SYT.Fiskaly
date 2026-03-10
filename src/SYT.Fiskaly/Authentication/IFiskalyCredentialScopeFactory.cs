using SYT.Fiskaly.Authentication.Credentials;

namespace SYT.Fiskaly.Authentication;

/// <summary>
/// Provides an ambient credential override for fiskaly HTTP pipelines.
/// </summary>
public interface IFiskalyCredentialScopeFactory
{
    /// <summary>
    /// Gets the current ambient credentials for the async flow.
    /// </summary>
    IFiskalyCredentials? Current { get; }

    /// <summary>
    /// Temporarily overrides the credentials used by <see cref="Handlers.JwtAuthHandler"/>.
    /// </summary>
    IDisposable Use(IFiskalyCredentials credentials);
}
