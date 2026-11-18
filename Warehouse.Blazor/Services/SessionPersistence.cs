using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Warehouse.Blazor.Models;

namespace Warehouse.Blazor.Services;

public class SessionPersistence(
    ProtectedSessionStorage storage,
    TokenStore tokens,
    JwtAuthStateProvider auth)
{
    private const string Key = "warehouse.auth";

    public async Task SaveAsync(SessionInfo session)
    {
        tokens.Set(session);
        auth.Notify();
        await storage.SetAsync(Key, session);
    }

    public async Task<bool> TryRestoreAsync()
    {
        if (tokens.IsAuthenticated)
        {
            return true;
        }

        try
        {
            var result = await storage.GetAsync<SessionInfo>(Key);
            if (!result.Success || result.Value is null)
            {
                return false;
            }

            if (result.Value.ExpiresAtUtc <= DateTime.UtcNow ||
                string.IsNullOrWhiteSpace(result.Value.AccessToken))
            {
                await ClearAsync();
                return false;
            }

            tokens.Set(result.Value);
            auth.Notify();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ClearAsync()
    {
        tokens.Clear();
        auth.Notify();
        try
        {
            await storage.DeleteAsync(Key);
        }
        catch
        {
            // ignore
        }
    }
}
