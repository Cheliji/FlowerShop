using FlowerShop.Api.Data;
using FlowerShop.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FlowerShop.Api.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CartItemResponse> AddToCartAsync(long userId, AddToCartRequest request)
    {
        var flower = await _db.Flowers.FindAsync(request.ProductId);
        if (flower == null) throw new Exception("商品不存在");

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId
                && c.FlowerId == request.ProductId
                && c.SelectedOptionQty == request.SkuId);

        if (existing != null)
        {
            existing.Count += request.Count;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.CartItems.Add(new Entities.CartItem
            {
                UserId = userId,
                FlowerId = request.ProductId,
                SelectedOptionQty = request.SkuId,
                Count = request.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        var cartCount = await _db.CartItems
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Count);

        return new CartItemResponse { CartCount = cartCount };
    }

    public async Task<List<CartListResponse>> GetCartListAsync(long userId)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Flower)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var result = new List<CartListResponse>();

        foreach (var item in cartItems)
        {
            var flower = item.Flower;
            var priceOptions = string.IsNullOrWhiteSpace(flower.PriceOptionsJson)
                ? new List<Entities.FlowerPriceOption>()
                : JsonSerializer.Deserialize<List<Entities.FlowerPriceOption>>(flower.PriceOptionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Entities.FlowerPriceOption>();

            var selectedOption = priceOptions.ElementAtOrDefault(item.SelectedOptionQty - 1);

            result.Add(new CartListResponse
            {
                Id = item.Id,
                ProductId = flower.Id,
                SkuId = item.SelectedOptionQty,
                ProductName = flower.Name,
                ProductImage = flower.MainImage,
                SpecName = selectedOption?.Label ?? "默认规格",
                Price = selectedOption?.Price ?? flower.Price,
                Count = item.Count,
                Stock = flower.Stock,
            });
        }

        return result;
    }

    public async Task UpdateCartItemAsync(long userId, long cartItemId, int count)
    {
        if (count < 1) throw new Exception("数量不能小于1");

        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item == null) throw new Exception("购物车商品不存在");

        item.Count = count;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCartItemAsync(long userId, long cartItemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item == null) throw new Exception("购物车商品不存在");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(long userId)
    {
        var items = await _db.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
