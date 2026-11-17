using WarehouseApi.Domain.Entities;

namespace WarehouseApi.Repositories;

public interface IUserRepository
{
    Task<AppUser?> FindByUserNameAsync(string userName, CancellationToken ct = default);
}
