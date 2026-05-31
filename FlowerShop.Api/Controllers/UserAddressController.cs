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
public class UserAddressController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserAddressController(AppDbContext db)
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
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var addresses = await _db.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new AddressResponse
            {
                Id = a.Id,
                ReceiverName = a.ReceiverName,
                Phone = a.Phone,
                Province = a.Province,
                City = a.City,
                District = a.District,
                DetailAddress = a.DetailAddress,
                IsDefault = a.IsDefault,
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AddressResponse>>.Success(addresses));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        if (request.IsDefault)
        {
            var existingDefaults = await _db.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var d in existingDefaults)
                d.IsDefault = false;
        }

        var address = new Entities.UserAddress
        {
            UserId = userId,
            ReceiverName = request.ReceiverName,
            Phone = request.Phone,
            Province = request.Province,
            City = request.City,
            District = request.District,
            DetailAddress = request.DetailAddress,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
        };

        _db.UserAddresses.Add(address);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Success(null, "添加成功"));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(long id, [FromBody] UpdateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address == null) return NotFound(ApiResponse<object>.Fail(404, "地址不存在"));

        if (request.IsDefault && !address.IsDefault)
        {
            var existingDefaults = await _db.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var d in existingDefaults)
                d.IsDefault = false;
        }

        address.ReceiverName = request.ReceiverName;
        address.Phone = request.Phone;
        address.Province = request.Province;
        address.City = request.City;
        address.District = request.District;
        address.DetailAddress = request.DetailAddress;
        address.IsDefault = request.IsDefault;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Success(null, "更新成功"));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address == null) return NotFound(ApiResponse<object>.Fail(404, "地址不存在"));

        _db.UserAddresses.Remove(address);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Success(null, "删除成功"));
    }
}
