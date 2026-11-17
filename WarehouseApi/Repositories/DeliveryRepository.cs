using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public class DeliveryRepository(WarehouseDbContext db) : IDeliveryRepository
{
    public async Task<PagedResult<Delivery>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<Delivery> q = db.Deliveries
            .AsNoTracking()
            .Include(d => d.Supplier)
            .Include(d => d.Items)
            .ThenInclude(i => i.Product);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(d =>
                d.DocumentNumber.ToLower().Contains(term) ||
                (d.Supplier != null && d.Supplier.Name.ToLower().Contains(term)));
        }

        q = (query.SortBy?.ToLower()) switch
        {
            "document" => query.Desc ? q.OrderByDescending(d => d.DocumentNumber) : q.OrderBy(d => d.DocumentNumber),
            "supplier" => query.Desc ? q.OrderByDescending(d => d.Supplier!.Name) : q.OrderBy(d => d.Supplier!.Name),
            _ => query.Desc ? q.OrderByDescending(d => d.DeliveryDate) : q.OrderBy(d => d.DeliveryDate)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResult<Delivery>(items, page, pageSize, total, totalPages);
    }

    public Task<Delivery?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Deliveries
            .Include(d => d.Supplier)
            .Include(d => d.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddAsync(Delivery delivery, CancellationToken ct = default)
    {
        db.Deliveries.Add(delivery);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Delivery delivery, CancellationToken ct = default)
    {
        db.Deliveries.Remove(delivery);
        await db.SaveChangesAsync(ct);
    }
}
