using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Mapping;

/// <summary>
/// Ручной маппинг сущностей в DTO (без AutoMapper).
/// </summary>
public static class EntityMapper
{
    public static ProductDto ToDto(Product product)
    {
        var qty = product.StockBalance?.Quantity ?? 0m;
        return new ProductDto(
            product.Id,
            product.Name,
            product.Article,
            product.Unit,
            product.Category,
            product.MinStockLevel,
            qty,
            qty < product.MinStockLevel);
    }

    public static StockDto ToDto(StockBalance stock)
    {
        var product = stock.Product!;
        return new StockDto(
            stock.ProductId,
            product.Article,
            product.Name,
            stock.Quantity,
            product.Unit,
            product.MinStockLevel,
            stock.Quantity < product.MinStockLevel,
            stock.LastUpdated);
    }

    public static SupplierDto ToDto(Supplier supplier) =>
        new(supplier.Id, supplier.Name, supplier.ContactPerson, supplier.Phone, supplier.Email, supplier.Address);

    public static DeliveryDto ToDto(Delivery delivery) =>
        new(
            delivery.Id,
            delivery.SupplierId,
            delivery.Supplier?.Name,
            delivery.DeliveryDate,
            delivery.DocumentNumber,
            delivery.Notes,
            delivery.Items.Select(i => new DeliveryItemDto(
                i.ProductId,
                i.Product?.Article,
                i.Product?.Name,
                i.Quantity,
                i.UnitPrice)).ToList());
}
