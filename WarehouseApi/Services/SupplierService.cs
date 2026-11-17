using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Mapping;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services;

public class SupplierService(ISupplierRepository suppliers, ILogger<SupplierService> logger) : ISupplierService
{
    public async Task<PagedResult<SupplierDto>> GetAsync(QueryParams query, CancellationToken ct = default)
    {
        var page = await suppliers.GetPagedAsync(query, ct);
        var items = page.Items.Select(EntityMapper.ToDto).ToList();
        return new PagedResult<SupplierDto>(items, page.Page, page.PageSize, page.TotalCount, page.TotalPages);
    }

    public async Task<SupplierDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var supplier = await suppliers.GetByIdAsync(id, ct);
        return supplier is null ? null : EntityMapper.ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(SupplierCreateDto dto, CancellationToken ct = default)
    {
        var supplier = new Supplier
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address?.Trim()
        };

        await suppliers.AddAsync(supplier, ct);
        logger.LogInformation("Создан поставщик {Name} (Id={Id})", supplier.Name, supplier.Id);
        return EntityMapper.ToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(int id, SupplierUpdateDto dto, CancellationToken ct = default)
    {
        var supplier = await suppliers.GetByIdAsync(id, ct);
        if (supplier is null) return null;

        supplier.Name = dto.Name.Trim();
        supplier.ContactPerson = dto.ContactPerson?.Trim();
        supplier.Phone = dto.Phone?.Trim();
        supplier.Email = dto.Email?.Trim();
        supplier.Address = dto.Address?.Trim();

        await suppliers.UpdateAsync(supplier, ct);
        return EntityMapper.ToDto(supplier);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var supplier = await suppliers.GetByIdAsync(id, ct);
        if (supplier is null) return false;

        if (await suppliers.HasDeliveriesAsync(id, ct))
        {
            throw new InvalidOperationException("Нельзя удалить поставщика, у которого есть поставки.");
        }

        await suppliers.DeleteAsync(supplier, ct);
        return true;
    }
}
