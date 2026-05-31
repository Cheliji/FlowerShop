using System.Security.Claims;
using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            return 0;
        return userId;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var result = await _orderService.CreateOrderAsync(userId, request);
        return Ok(ApiResponse<OrderResponse>.Success(result));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetOrderList([FromQuery] byte? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var result = await _orderService.GetOrderListAsync(userId, status, page, pageSize);
        return Ok(ApiResponse<List<OrderResponse>>.Success(result));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetail(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var result = await _orderService.GetOrderDetailAsync(userId, id);
        if (result == null) return NotFound(ApiResponse<object>.Fail(404, "订单不存在"));

        return Ok(ApiResponse<OrderResponse>.Success(result));
    }

    [Authorize]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayOrder(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        await _orderService.PayOrderAsync(userId, id);
        return Ok(ApiResponse<object>.Success(null));
    }

    [Authorize]
    [HttpPost("{id}/receive")]
    public async Task<IActionResult> ReceiveOrder(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        await _orderService.ReceiveOrderAsync(userId, id);
        return Ok(ApiResponse<object>.Success(null, "确认收货成功"));
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        await _orderService.CancelOrderAsync(userId, id);
        return Ok(ApiResponse<object>.Success(null));
    }
}
