namespace FlowerShop.Api.Entities;

public enum Gender : byte
{
    Unknown = 0,
    Male = 1,
    Female = 2
}

public enum OrderStatus : byte
{
    PendingPayment = 0,
    Paid = 1,
    Shipped = 2,
    Completed = 3,
    Cancelled = 4
}
