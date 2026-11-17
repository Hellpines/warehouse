using WarehouseApi.DTOs;

namespace WarehouseApi.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
