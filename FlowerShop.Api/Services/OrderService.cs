using System.Text.Json;
using FlowerShop.Api.Data;
using FlowerShop.Api.Entities;
using FlowerShop.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 创建订单。
    /// 库存策略：下单减库存（创建订单时立即扣减可用库存并锁定，支付时转移到已售）。
    /// 优点：避免超卖；缺点：未付款订单会占用库存。
    /// </summary>
    public async Task<OrderResponse> CreateOrderAsync(long userId, CreateOrderRequest request)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Flower)
            .Where(c => c.UserId == userId && request.CartItemIds.Contains(c.Id))
            .ToListAsync();

        if (cartItems.Count == 0)
            throw new Exception("购物车为空，请选择商品");

        var address = await _db.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);

        if (address == null)
            throw new Exception("收货地址不存在");

        // 计算每个商品的总库存消耗
        var flowerConsumptions = new Dictionary<long, int>();
        decimal totalAmount = 0;
        var orderItemsData = new List<(CartItem CartItem, FlowerPriceOption? PriceOption, decimal SubTotal)>();

        foreach (var cartItem in cartItems)
        {
            var flower = cartItem.Flower;
            var priceOptions = string.IsNullOrWhiteSpace(flower.PriceOptionsJson)
                ? new List<FlowerPriceOption>()
                : JsonSerializer.Deserialize<List<FlowerPriceOption>>(flower.PriceOptionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FlowerPriceOption>();

            var priceOption = priceOptions.ElementAtOrDefault(cartItem.SelectedOptionQty - 1);
            var unitPrice = priceOption?.Price ?? flower.Price;
            var actualQuantity = priceOption?.Quantity ?? 1; // 实际枝数/份数
            var subTotal = unitPrice * cartItem.Count;
            totalAmount += subTotal;

            var consumeStock = actualQuantity * cartItem.Count;
            if (flowerConsumptions.ContainsKey(flower.Id))
                flowerConsumptions[flower.Id] += consumeStock;
            else
                flowerConsumptions[flower.Id] = consumeStock;

            orderItemsData.Add((cartItem, priceOption, subTotal));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 检查并扣减库存
            foreach (var (flowerId, consume) in flowerConsumptions)
            {
                var inventory = await _db.FlowerInventories
                    .FirstOrDefaultAsync(fi => fi.FlowerId == flowerId);

                if (inventory == null)
                {
                    var flower = await _db.Flowers.FindAsync(flowerId);
                    if (flower == null)
                        throw new Exception("商品不存在");

                    inventory = new FlowerInventory
                    {
                        FlowerId = flowerId,
                        StockQuantity = flower.Stock,
                        AvailableQuantity = flower.Stock,
                        LockedQuantity = 0,
                        SoldQuantity = 0,
                        LastUpdated = DateTime.UtcNow,
                    };
                    _db.FlowerInventories.Add(inventory);
                }

                if (inventory.AvailableQuantity < consume)
                    throw new Exception("库存不足，请减少购买数量");

                inventory.AvailableQuantity -= consume;
                inventory.LockedQuantity += consume;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            // 生成订单号
            var orderNo = GenerateOrderNo();

            // 创建订单
            var order = new Order
            {
                OrderNo = orderNo,
                UserId = userId,
                Status = OrderStatus.PendingPayment,
                TotalAmount = totalAmount,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.Phone,
                ReceiverAddress = $"{address.Province}{address.City}{address.District}{address.DetailAddress}",
                DeliveryDate = request.DeliveryDate,
                DeliveryTimeSlot = request.DeliveryTimeSlot,
                CardMessage = request.CardMessage,
                Remark = request.Remark,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // 创建订单项
            foreach (var (cartItem, priceOption, subTotal) in orderItemsData)
            {
                var flower = cartItem.Flower;
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    FlowerId = flower.Id,
                    FlowerName = flower.Name,
                    FlowerImage = flower.MainImage,
                    PriceOptionSnapshot = priceOption != null ? JsonSerializer.Serialize(priceOption) : null,
                    Quantity = cartItem.Count,
                    UnitPrice = priceOption?.Price ?? flower.Price,
                    SubTotal = subTotal,
                });
            }

            // 删除购物车项
            _db.CartItems.RemoveRange(cartItems);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetOrderDetailAsync(userId, order.Id) ?? throw new Exception("订单创建失败");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse?> GetOrderDetailAsync(long userId, long orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) return null;

        return MapToResponse(order);
    }

    public async Task PayOrderAsync(long userId, long orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) throw new Exception("订单不存在");
        if (order.Status != OrderStatus.PendingPayment) throw new Exception("订单状态不允许支付");

        // 支付时：将锁定库存转为已售库存
        foreach (var item in order.OrderItems)
        {
            var inventory = await _db.FlowerInventories
                .FirstOrDefaultAsync(fi => fi.FlowerId == item.FlowerId);

            if (inventory != null)
            {
                // 计算该订单项消耗的实际库存数量（从快照中解析 quantity）
                int actualQty = item.Quantity;
                if (!string.IsNullOrWhiteSpace(item.PriceOptionSnapshot))
                {
                    var option = JsonSerializer.Deserialize<FlowerPriceOption>(item.PriceOptionSnapshot, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (option != null) actualQty = option.Quantity * item.Quantity;
                }

                inventory.LockedQuantity -= actualQty;
                inventory.SoldQuantity += actualQty;
                inventory.LastUpdated = DateTime.UtcNow;
            }
        }

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task CancelOrderAsync(long userId, long orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) throw new Exception("订单不存在");
        if (order.Status != OrderStatus.PendingPayment) throw new Exception("只有未付款订单可取消");

        // 取消时：返还锁定库存到可用库存
        foreach (var item in order.OrderItems)
        {
            var inventory = await _db.FlowerInventories
                .FirstOrDefaultAsync(fi => fi.FlowerId == item.FlowerId);

            if (inventory != null)
            {
                int actualQty = item.Quantity;
                if (!string.IsNullOrWhiteSpace(item.PriceOptionSnapshot))
                {
                    var option = JsonSerializer.Deserialize<FlowerPriceOption>(item.PriceOptionSnapshot, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (option != null) actualQty = option.Quantity * item.Quantity;
                }

                inventory.AvailableQuantity += actualQty;
                inventory.LockedQuantity -= actualQty;
                inventory.LastUpdated = DateTime.UtcNow;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<OrderResponse>> GetOrderListAsync(long userId, byte? status, int page, int pageSize)
    {
        var query = _db.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == (OrderStatus)status.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return orders.Select(MapToResponse).ToList();
    }

    public async Task ReceiveOrderAsync(long userId, long orderId)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) throw new Exception("订单不存在");
        if (order.Status != OrderStatus.Shipped) throw new Exception("订单状态不允许确认收货");

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static string GenerateOrderNo()
    {
        return DateTime.UtcNow.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            Status = (byte)order.Status,
            StatusText = GetStatusText(order.Status),
            TotalAmount = order.TotalAmount,
            ReceiverName = order.ReceiverName,
            ReceiverPhone = order.ReceiverPhone,
            ReceiverAddress = order.ReceiverAddress,
            DeliveryDate = order.DeliveryDate,
            DeliveryTimeSlot = order.DeliveryTimeSlot,
            CardMessage = order.CardMessage,
            Remark = order.Remark,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            Items = order.OrderItems.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                FlowerId = i.FlowerId,
                FlowerName = i.FlowerName,
                FlowerImage = i.FlowerImage,
                SpecName = !string.IsNullOrWhiteSpace(i.PriceOptionSnapshot)
                    ? JsonSerializer.Deserialize<FlowerPriceOption>(i.PriceOptionSnapshot, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Label
                    : null,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal,
            }).ToList(),
        };
    }

    private static string GetStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.PendingPayment => "待付款",
            OrderStatus.Paid => "已付款",
            OrderStatus.Shipped => "已发货",
            OrderStatus.Completed => "已完成",
            OrderStatus.Cancelled => "已取消",
            _ => "未知",
        };
    }
}
