using SYT.Fiskaly.Authentication.Models;
using SYT.Fiskaly.Authentication.ValueObjects;

namespace SYT.Fiskaly.Authentication.Credentials;

public sealed class ApiKeyCredentials : IFiskalyCredentials
{
    public ApiKeyCredentials(ApiKey apiKey, ApiSecret apiSecret)
    {
        ApiKey = apiKey;
        ApiSecret = apiSecret;
    }

    public ApiKey ApiKey { get; }

    public ApiSecret ApiSecret { get; }

    public AuthenticationPayload CreatePayload()
    {
        return new ApiKeyAuthenticationPayload(ApiKey, ApiSecret);
    }
}
