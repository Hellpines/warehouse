using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApp.Models;

/// <summary>
/// Поставка товаров от поставщика.
/// </summary>
[Table("Deliveries")]
public class Delivery
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [Required]
    public DateTime DeliveryDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Связь one-to-many: поставка содержит несколько позиций.
    /// </summary>
    public ICollection<DeliveryItem> Items { get; set; } = new List<DeliveryItem>();
}
