using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApp.Models;

/// <summary>
/// Поставщик товаров на склад.
/// </summary>
[Table("Suppliers")]
public class Supplier
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    /// <summary>
    /// Связь one-to-many: у поставщика много поставок.
    /// </summary>
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    /// <summary>
    /// Связь many-to-many с товарами через промежуточную сущность ProductSupplier.
    /// </summary>
    public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
}
