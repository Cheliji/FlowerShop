using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FlowerShop.Api.Entities;

public class FlowerImageItem
{
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}

public class FlowerPriceOption
{
    public int Quantity { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

[Table("Flowers")]
public class Flower
{
    [Key]
    public long Id { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Subtitle { get; set; }

    [StringLength(200)]
    public string? FlowerLanguage { get; set; }

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? SuitableFor { get; set; }

    [StringLength(500)]
    public string? DeliveryDesc { get; set; }

    [Required]
    [StringLength(500)]
    public string MainImage { get; set; } = string.Empty;

    [Column(TypeName = "json")]
    public string ImagesJson { get; set; } = "[]";

    [NotMapped]
    public List<FlowerImageItem> Images
    {
        get => string.IsNullOrWhiteSpace(ImagesJson)
            ? new List<FlowerImageItem>()
            : JsonSerializer.Deserialize<List<FlowerImageItem>>(ImagesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FlowerImageItem>();
        set => ImagesJson = JsonSerializer.Serialize(value);
    }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalPrice { get; set; }

    [Column(TypeName = "json")]
    public string PriceOptionsJson { get; set; } = "[]";

    [NotMapped]
    public List<FlowerPriceOption> PriceOptions
    {
        get => string.IsNullOrWhiteSpace(PriceOptionsJson)
            ? new List<FlowerPriceOption>()
            : JsonSerializer.Deserialize<List<FlowerPriceOption>>(PriceOptionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FlowerPriceOption>();
        set => PriceOptionsJson = JsonSerializer.Serialize(value);
    }

    public int Stock { get; set; } = 0;

    public int SoldCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
