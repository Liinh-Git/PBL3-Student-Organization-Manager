using System.Net.Http.Headers;

namespace Org.Frontend.Services.Auth;

public class AuthHeaderDelegatingHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;

    public AuthHeaderDelegatingHandler(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenStorage.GetTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (InvalidOperationException)
        {
            // Ignore JSInterop exceptions if running during Prerendering
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
