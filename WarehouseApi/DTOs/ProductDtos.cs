using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Article,
    string Unit,
    string? Category,
    decimal MinStockLevel,
    decimal Quantity,
    bool IsLowStock);

public class ProductCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Article { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = "шт";

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinStockLevel { get; set; }
}

public class ProductUpdateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = "шт";

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinStockLevel { get; set; }
}
