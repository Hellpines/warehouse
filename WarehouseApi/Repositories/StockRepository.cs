using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public class StockRepository(WarehouseDbContext db) : IStockRepository
{
    public async Task<PagedResult<StockBalance>> GetPagedAsync(QueryParams query, bool? lowOnly, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<StockBalance> q = db.StockBalances.AsNoTracking().Include(s => s.Product);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(s =>
                s.Product!.Name.ToLower().Contains(term) ||
                s.Product.Article.ToLower().Contains(term));
        }

        if (lowOnly == true)
        {
            q = q.Where(s => s.Quantity < s.Product!.MinStockLevel);
        }

        // SQLite не поддерживает ORDER BY по decimal — приводим к double.
        q = (query.SortBy?.ToLower()) switch
        {
            "name" => query.Desc ? q.OrderByDescending(s => s.Product!.Name) : q.OrderBy(s => s.Product!.Name),
            "article" => query.Desc ? q.OrderByDescending(s => s.Product!.Article) : q.OrderBy(s => s.Product!.Article),
            _ => query.Desc
                ? q.OrderByDescending(s => (double)s.Quantity)
                : q.OrderBy(s => (double)s.Quantity)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResult<StockBalance>(items, page, pageSize, total, totalPages);
    }

    public Task<StockBalance?> GetByProductIdAsync(int productId, CancellationToken ct = default) =>
        db.StockBalances.Include(s => s.Product).FirstOrDefaultAsync(s => s.ProductId == productId, ct);

    public Task SaveAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
