namespace FlowerShop.Api.Models.DTOs;

public class CreateOrderRequest
{
    public long AddressId { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryTimeSlot { get; set; }
    public string? CardMessage { get; set; }
    public string? Remark { get; set; }
    public List<long> CartItemIds { get; set; } = new();
}

public class OrderResponse
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryTimeSlot { get; set; }
    public string? CardMessage { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public long Id { get; set; }
    public long FlowerId { get; set; }
    public string FlowerName { get; set; } = string.Empty;
    public string? FlowerImage { get; set; }
    public string? SpecName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class AddressResponse
{
    public long Id { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string DetailAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
