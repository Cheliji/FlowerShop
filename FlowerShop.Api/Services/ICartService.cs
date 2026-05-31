using FlowerShop.Api.Models.DTOs;

namespace FlowerShop.Api.Services;

public interface ICartService
{
    Task<CartItemResponse> AddToCartAsync(long userId, AddToCartRequest request);
    Task<List<CartListResponse>> GetCartListAsync(long userId);
    Task UpdateCartItemAsync(long userId, long cartItemId, int count);
    Task DeleteCartItemAsync(long userId, long cartItemId);
    Task ClearCartAsync(long userId);
}
