using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;

namespace WarehouseApi.Repositories;

public class UserRepository(WarehouseDbContext db) : IUserRepository
{
    public Task<AppUser?> FindByUserNameAsync(string userName, CancellationToken ct = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == userName, ct);
}
