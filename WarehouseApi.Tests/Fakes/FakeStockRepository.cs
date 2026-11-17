using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Repositories;

namespace WarehouseApi.Tests.Fakes;

public class FakeStockRepository : IStockRepository
{
    public StockBalance? Entity { get; set; }
    public int SaveCalls { get; private set; }

    public Task<PagedResult<StockBalance>> GetPagedAsync(QueryParams query, bool? lowOnly, CancellationToken ct = default)
    {
        var items = Entity is null ? new List<StockBalance>() : [Entity];
        return Task.FromResult(new PagedResult<StockBalance>(items, 1, items.Count, items.Count, 1));
    }

    public Task<StockBalance?> GetByProductIdAsync(int productId, CancellationToken ct = default) =>
        Task.FromResult(Entity is not null && Entity.ProductId == productId ? Entity : null);

    public Task SaveAsync(CancellationToken ct = default)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }
}
