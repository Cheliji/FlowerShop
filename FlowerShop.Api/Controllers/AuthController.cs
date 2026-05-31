using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .FirstOrDefault()?.ErrorMessage ?? "请求参数错误";
            return BadRequest(ApiResponse<object>.Fail(400, firstError));
        }

        var result = await _authService.RegisterAsync(request);
        if (result.Code != 200)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .FirstOrDefault()?.ErrorMessage ?? "请求参数错误";
            return BadRequest(ApiResponse<object>.Fail(400, firstError));
        }

        var result = await _authService.LoginAsync(request);
        if (result.Code != 200)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(ApiResponse<object>.Fail(401, "登录已过期，请重新登录"));

        var result = await _authService.GetProfileAsync(userId);
        if (result.Code != 200)
            return StatusCode(result.Code, result);

        return Ok(result);
    }
}
