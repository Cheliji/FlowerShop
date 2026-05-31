namespace FlowerShop.Api.Models.DTOs;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class ProductListRequest
{
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductListResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? FlowerLanguage { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public string MainImage { get; set; } = string.Empty;
    public int SoldCount { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? FlowerLanguage { get; set; }
    public string? Description { get; set; }
    public string? SuitableFor { get; set; }
    public string? DeliveryDesc { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int Stock { get; set; }
    public int SoldCount { get; set; }
    public string MainImage { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public List<FlowerPriceOptionDto> PriceOptions { get; set; } = new();
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class FlowerPriceOptionDto
{
    public int Quantity { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class SkuResponse
{
    public int Id { get; set; }
    public string SpecName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
