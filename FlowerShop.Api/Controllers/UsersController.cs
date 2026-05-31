using System.Security.Claims;
using FlowerShop.Api.Data;
using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
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
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound(ApiResponse<object>.Fail(404, "用户不存在"));

        var result = new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Gender = user.Gender,
            Phone = user.Phone,
        };

        return Ok(ApiResponse<UserProfileResponse>.Success(result));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(ApiResponse<object>.Fail(404, "用户不存在"));

        if (request.Nickname != null)
            user.Nickname = request.Nickname;
        if (request.Avatar != null)
            user.Avatar = request.Avatar;
        if (request.Phone != null)
            user.Phone = request.Phone;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Success(null, "更新成功"));
    }
}
