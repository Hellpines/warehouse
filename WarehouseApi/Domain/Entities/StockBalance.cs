using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApi.Domain.Entities;

[Table("StockBalances")]
public class StockBalance
{
    [Key]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
