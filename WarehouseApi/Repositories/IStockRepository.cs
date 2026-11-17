using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public interface IStockRepository
{
    Task<PagedResult<StockBalance>> GetPagedAsync(QueryParams query, bool? lowOnly, CancellationToken ct = default);
    Task<StockBalance?> GetByProductIdAsync(int productId, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
