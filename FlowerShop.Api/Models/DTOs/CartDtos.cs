namespace FlowerShop.Api.Models.DTOs;

public class AddToCartRequest
{
    public long ProductId { get; set; }
    public int SkuId { get; set; }
    public int Count { get; set; } = 1;
}

public class UpdateCartRequest
{
    public int Count { get; set; }
}

public class CartItemResponse
{
    public int CartCount { get; set; }
}

public class CartListResponse
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public int SkuId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }
    public int Stock { get; set; }
}
