using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApi.Domain.Entities;

[Table("DeliveryItems")]
public class DeliveryItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DeliveryId { get; set; }

    [ForeignKey(nameof(DeliveryId))]
    public Delivery? Delivery { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}
