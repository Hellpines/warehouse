using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApi.Domain.Entities;

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("Sku")]
    public string Article { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = "шт";

    [MaxLength(100)]
    public string? Category { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal MinStockLevel { get; set; }

    public ICollection<DeliveryItem> DeliveryItems { get; set; } = new List<DeliveryItem>();
    public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    public StockBalance? StockBalance { get; set; }
}
