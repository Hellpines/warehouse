using System.Net.Http.Headers;

namespace Warehouse.Blazor.Services;

public class AuthHeaderHandler(TokenStore tokens) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (tokens.IsAuthenticated && tokens.Session is not null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokens.Session.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
