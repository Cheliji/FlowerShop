using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerShop.Api.Entities;

[Table("CartItems")]
public class CartItem
{
    [Key]
    public long Id { get; set; }

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public long FlowerId { get; set; }

    [ForeignKey(nameof(FlowerId))]
    public Flower Flower { get; set; } = null!;

    public int SelectedOptionQty { get; set; } = 1;

    public int Count { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
