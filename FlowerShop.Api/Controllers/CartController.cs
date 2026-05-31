using System.Security.Claims;
using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
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
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var result = await _cartService.AddToCartAsync(userId, request);
        return Ok(ApiResponse<CartItemResponse>.Success(result));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCartList()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var result = await _cartService.GetCartListAsync(userId);
        return Ok(ApiResponse<List<CartListResponse>>.Success(result));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCartItem(long id, [FromBody] UpdateCartRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        await _cartService.UpdateCartItemAsync(userId, id, request.Count);
        return Ok(ApiResponse<object>.Success(null));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        await _cartService.DeleteCartItemAsync(userId, id);
        return Ok(ApiResponse<object>.Success(null));
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        await _cartService.ClearCartAsync(userId);
        return Ok(ApiResponse<object>.Success(null));
    }
}
