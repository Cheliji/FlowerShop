namespace FlowerShop.Api.Models.DTOs;

public class UpdateUserRequest
{
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
}

public class CreateAddressRequest
{
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string DetailAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class UpdateAddressRequest
{
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string DetailAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
