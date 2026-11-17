using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.DTOs;

public record StockDto(
    int ProductId,
    string Article,
    string Name,
    decimal Quantity,
    string Unit,
    decimal MinStockLevel,
    bool IsLowStock,
    DateTime LastUpdated);

public class StockAdjustDto
{
    [Required]
    [Range(typeof(decimal), "0", "999999999")]
    public decimal Quantity { get; set; }
}
