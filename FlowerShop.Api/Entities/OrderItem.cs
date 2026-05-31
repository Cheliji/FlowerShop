using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerShop.Api.Entities;

[Table("OrderItems")]
public class OrderItem
{
    [Key]
    public long Id { get; set; }

    public long OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    public long FlowerId { get; set; }

    [Required]
    [StringLength(100)]
    public string FlowerName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? FlowerImage { get; set; }

    [Column(TypeName = "json")]
    public string? PriceOptionSnapshot { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
}
