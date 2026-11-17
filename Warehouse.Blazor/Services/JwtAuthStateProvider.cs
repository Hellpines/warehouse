using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Warehouse.Blazor.Services;

public class JwtAuthStateProvider(TokenStore tokens) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!tokens.IsAuthenticated || tokens.Session is null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var session = tokens.Session;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, session.UserName),
            new(ClaimTypes.Role, session.Role),
            new("display_name", session.DisplayName ?? session.UserName)
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void Notify() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
