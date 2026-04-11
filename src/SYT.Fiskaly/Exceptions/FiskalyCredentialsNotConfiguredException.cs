namespace SYT.Fiskaly.Exceptions;

public sealed class FiskalyCredentialsNotConfiguredException : FiskalyException
{
    public FiskalyCredentialsNotConfiguredException()
        : base("No default Fiskaly credentials are configured. Configure 'Fiskaly:ApiKey' and 'Fiskaly:ApiSecret' or execute the request inside a credential scope.")
    {
    }
}
