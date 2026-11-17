using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public class SupplierRepository(WarehouseDbContext db) : ISupplierRepository
{
    public async Task<PagedResult<Supplier>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<Supplier> q = db.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(s =>
                s.Name.ToLower().Contains(term) ||
                (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(term)));
        }

        q = query.Desc ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResult<Supplier>(items, page, pageSize, total, totalPages);
    }

    public Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Suppliers.Update(supplier);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Suppliers.Remove(supplier);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> HasDeliveriesAsync(int supplierId, CancellationToken ct = default) =>
        db.Deliveries.AnyAsync(d => d.SupplierId == supplierId, ct);
}
