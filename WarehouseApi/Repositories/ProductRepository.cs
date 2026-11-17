using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public class ProductRepository(WarehouseDbContext db) : IProductRepository
{
    public async Task<PagedResult<Product>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<Product> q = db.Products.AsNoTracking().Include(p => p.StockBalance);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Article.ToLower().Contains(term) ||
                (p.Category != null && p.Category.ToLower().Contains(term)));
        }

        q = (query.SortBy?.ToLower()) switch
        {
            "article" => query.Desc ? q.OrderByDescending(p => p.Article) : q.OrderBy(p => p.Article),
            "category" => query.Desc ? q.OrderByDescending(p => p.Category) : q.OrderBy(p => p.Category),
            "quantity" => query.Desc
                ? q.OrderByDescending(p => (double)p.StockBalance!.Quantity)
                : q.OrderBy(p => (double)p.StockBalance!.Quantity),
            _ => query.Desc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<Product>(items, page, pageSize, total, totalPages);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Products.Include(p => p.StockBalance).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetByArticleAsync(string article, CancellationToken ct = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Article == article, ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Update(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> HasDeliveryItemsAsync(int productId, CancellationToken ct = default) =>
        db.DeliveryItems.AnyAsync(i => i.ProductId == productId, ct);
}
