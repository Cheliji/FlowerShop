using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerShop.Api.Entities;

[Table("FlowerInventories")]
public class FlowerInventory
{
    [Key]
    public long Id { get; set; }

    public long FlowerId { get; set; }

    [ForeignKey(nameof(FlowerId))]
    public Flower Flower { get; set; } = null!;

    public int StockQuantity { get; set; } = 0;

    public int AvailableQuantity { get; set; } = 0;

    public int LockedQuantity { get; set; } = 0;

    public int SoldQuantity { get; set; } = 0;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
