using System.Net;
using SYT.Fiskaly.Authentication;
using SYT.Fiskaly.Authentication.Credentials;
using SYT.Fiskaly.Exceptions;

namespace SYT.Fiskaly.Handlers;

internal sealed class JwtAuthHandler(
    IFiskalyAuthenticationService authService,
    IFiskalyCredentialScopeFactory credentialScopeFactory,
    ILogger<JwtAuthHandler> logger)
    : DelegatingHandler
{
    private readonly IFiskalyAuthenticationService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    private readonly IFiskalyCredentialScopeFactory _credentialScopeFactory = credentialScopeFactory ?? throw new ArgumentNullException(nameof(credentialScopeFactory));
    private readonly ILogger<JwtAuthHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        IFiskalyCredentials? credentials = _credentialScopeFactory.Current;

        string token = credentials is not null
            ? await _authService.GetAccessTokenAsync(credentials, cancellationToken).ConfigureAwait(false)
            : await _authService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        _logger.LogJwtTokenAdded(request.Method, request.RequestUri);

        try
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (FiskalyApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            // The token was accepted by the cache and refused by the provider. This handler sits outside the
            // retry pipeline (see AddFiskalyPipeline for why), so the attempts of THIS call still carry the
            // rejected token - but the next one authenticates afresh instead of repeating the refusal until the
            // token's nominal expiry, which is what fiskaly's guidance for a 401 ("simply reauthorize") asks.
            _authService.InvalidateToken(credentials);
            throw;
        }
    }
}
