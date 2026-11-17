using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Mapping;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services;

public class DeliveryService(
    IDeliveryRepository deliveries,
    ISupplierRepository suppliers,
    IProductRepository products,
    WarehouseDbContext db,
    ILogger<DeliveryService> logger) : IDeliveryService
{
    public async Task<PagedResult<DeliveryDto>> GetAsync(QueryParams query, CancellationToken ct = default)
    {
        var page = await deliveries.GetPagedAsync(query, ct);
        var items = page.Items.Select(EntityMapper.ToDto).ToList();
        return new PagedResult<DeliveryDto>(items, page.Page, page.PageSize, page.TotalCount, page.TotalPages);
    }

    public async Task<DeliveryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var delivery = await deliveries.GetByIdAsync(id, ct);
        return delivery is null ? null : EntityMapper.ToDto(delivery);
    }

    public async Task<DeliveryDto> CreateAsync(DeliveryCreateDto dto, CancellationToken ct = default)
    {
        var supplier = await suppliers.GetByIdAsync(dto.SupplierId, ct);
        if (supplier is null)
        {
            throw new InvalidOperationException($"Поставщик Id={dto.SupplierId} не найден.");
        }

        foreach (var item in dto.Items)
        {
            var product = await products.GetByIdAsync(item.ProductId, ct);
            if (product is null)
            {
                throw new InvalidOperationException($"Товар Id={item.ProductId} не найден.");
            }
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var delivery = new Delivery
            {
                SupplierId = dto.SupplierId,
                DocumentNumber = dto.DocumentNumber.Trim(),
                DeliveryDate = dto.DeliveryDate?.ToUniversalTime() ?? DateTime.UtcNow.Date,
                Notes = dto.Notes?.Trim(),
                Items = dto.Items.Select(i => new DeliveryItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await deliveries.AddAsync(delivery, ct);
            DbSeeder.ApplyDeliveryToStock(db, delivery);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var created = await deliveries.GetByIdAsync(delivery.Id, ct)
                ?? throw new InvalidOperationException("Не удалось загрузить созданную поставку.");

            logger.LogInformation("Создана поставка {Document} (Id={Id})", created.DocumentNumber, created.Id);
            return EntityMapper.ToDto(created);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException("Не удалось сохранить поставку. Возможно, номер документа уже занят.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var delivery = await deliveries.GetByIdAsync(id, ct);
        if (delivery is null) return false;

        // Для учебной модели просто удаляем документ (остатки не откатываем автоматически).
        await deliveries.DeleteAsync(delivery, ct);
        logger.LogWarning("Удалена поставка Id={Id}", id);
        return true;
    }
}
