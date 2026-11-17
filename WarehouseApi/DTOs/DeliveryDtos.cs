using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.DTOs;

public record DeliveryItemDto(int ProductId, string? Article, string? ProductName, decimal Quantity, decimal UnitPrice);

public record DeliveryDto(
    int Id,
    int SupplierId,
    string? SupplierName,
    DateTime DeliveryDate,
    string DocumentNumber,
    string? Notes,
    IReadOnlyList<DeliveryItemDto> Items);

public class DeliveryItemCreateDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999")]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal UnitPrice { get; set; }
}

public class DeliveryCreateDto
{
    [Range(1, int.MaxValue)]
    public int SupplierId { get; set; }

    [Required]
    [MaxLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    public DateTime? DeliveryDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<DeliveryItemCreateDto> Items { get; set; } = [];
}
