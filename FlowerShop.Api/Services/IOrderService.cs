using FlowerShop.Api.Models.DTOs;

namespace FlowerShop.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(long userId, CreateOrderRequest request);
    Task<OrderResponse?> GetOrderDetailAsync(long userId, long orderId);
    Task<List<OrderResponse>> GetOrderListAsync(long userId, byte? status, int page, int pageSize);
    Task PayOrderAsync(long userId, long orderId);
    Task ReceiveOrderAsync(long userId, long orderId);
    Task CancelOrderAsync(long userId, long orderId);
}
