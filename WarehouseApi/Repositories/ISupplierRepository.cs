using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public interface ISupplierRepository
{
    Task<PagedResult<Supplier>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Supplier supplier, CancellationToken ct = default);
    Task UpdateAsync(Supplier supplier, CancellationToken ct = default);
    Task DeleteAsync(Supplier supplier, CancellationToken ct = default);
    Task<bool> HasDeliveriesAsync(int supplierId, CancellationToken ct = default);
}
