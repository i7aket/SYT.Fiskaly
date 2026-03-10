using System.Threading;
using SYT.Fiskaly.Authentication.Credentials;

namespace SYT.Fiskaly.Authentication;

internal sealed class FiskalyCredentialScopeFactory : IFiskalyCredentialScopeFactory
{
    private static readonly AsyncLocal<ScopeFrame?> CurrentFrame = new();

    public IFiskalyCredentials? Current => CurrentFrame.Value?.Credentials;

    public IDisposable Use(IFiskalyCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        ScopeFrame? previous = CurrentFrame.Value;
        CurrentFrame.Value = new ScopeFrame(credentials, previous);

        return new RestoreScope(() => CurrentFrame.Value = previous);
    }

    private sealed record ScopeFrame(IFiskalyCredentials Credentials, ScopeFrame? Previous);

    private sealed class RestoreScope(Action restore) : IDisposable
    {
        private Action? _restore = restore;

        public void Dispose()
        {
            Interlocked.Exchange(ref _restore, null)?.Invoke();
        }
    }
}
