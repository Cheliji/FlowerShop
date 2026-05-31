using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerShop.Api.Entities;

[Table("Orders")]
public class Order
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(32)]
    public string OrderNo { get; set; } = string.Empty;

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } = 0;

    [Required]
    [StringLength(50)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ReceiverPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ReceiverAddress { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Remark { get; set; }

    public DateTime? DeliveryDate { get; set; }

    [StringLength(20)]
    public string? DeliveryTimeSlot { get; set; }

    [StringLength(500)]
    public string? CardMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
