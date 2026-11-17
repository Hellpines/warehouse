using WarehouseApi.DTOs;
using WarehouseApi.Mapping;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services;

public class StockService(IStockRepository stock, ILogger<StockService> logger) : IStockService
{
    public async Task<PagedResult<StockDto>> GetAsync(QueryParams query, bool? lowOnly, CancellationToken ct = default)
    {
        var page = await stock.GetPagedAsync(query, lowOnly, ct);
        var items = page.Items.Select(EntityMapper.ToDto).ToList();
        return new PagedResult<StockDto>(items, page.Page, page.PageSize, page.TotalCount, page.TotalPages);
    }

    public async Task<StockDto?> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        var entity = await stock.GetByProductIdAsync(productId, ct);
        return entity is null ? null : EntityMapper.ToDto(entity);
    }

    public async Task<StockDto?> SetQuantityAsync(int productId, StockAdjustDto dto, CancellationToken ct = default)
    {
        var entity = await stock.GetByProductIdAsync(productId, ct);
        if (entity is null) return null;

        entity.Quantity = dto.Quantity;
        entity.LastUpdated = DateTime.UtcNow;
        await stock.SaveAsync(ct);
        logger.LogInformation("Остаток товара {ProductId} установлен в {Quantity}", productId, dto.Quantity);
        return EntityMapper.ToDto(entity);
    }
}
