using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApp.Models;

/// <summary>
/// Промежуточная сущность для связи many-to-many: товар ↔ поставщик.
/// </summary>
[Table("ProductSuppliers")]
public class ProductSupplier
{
    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// Признак основного поставщика для данного товара.
    /// </summary>
    public bool IsPrimary { get; set; }
}
