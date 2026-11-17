using Warehouse.Blazor.Models;

namespace Warehouse.Blazor.Services;

public class TokenStore
{
    public SessionInfo? Session { get; private set; }

    public bool IsAuthenticated =>
        Session is not null && Session.ExpiresAtUtc > DateTime.UtcNow;

    public event Action? Changed;

    public void Set(SessionInfo session)
    {
        Session = session;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Session = null;
        Changed?.Invoke();
    }
}
