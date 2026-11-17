using WarehouseApi.DTOs;

namespace WarehouseApi.Services;

public interface IStockService
{
    Task<PagedResult<StockDto>> GetAsync(QueryParams query, bool? lowOnly, CancellationToken ct = default);
    Task<StockDto?> GetByProductIdAsync(int productId, CancellationToken ct = default);
    Task<StockDto?> SetQuantityAsync(int productId, StockAdjustDto dto, CancellationToken ct = default);
}
