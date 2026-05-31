using FlowerShop.Api.Data;
using FlowerShop.Api.Entities;
using FlowerShop.Api.Helpers;
using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<object>> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username && !u.IsDeleted))
            return ApiResponse<object>.Fail(400, "用户名已存在");

        if (!string.IsNullOrEmpty(request.Phone) && await _context.Users.AnyAsync(u => u.Phone == request.Phone && !u.IsDeleted))
            return ApiResponse<object>.Fail(400, "手机号已注册");

        if (!string.IsNullOrEmpty(request.Email) && await _context.Users.AnyAsync(u => u.Email == request.Email && !u.IsDeleted))
            return ApiResponse<object>.Fail(400, "邮箱已注册");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Phone = request.Phone,
            Email = request.Email,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ApiResponse<object>.Success(null, "注册成功");
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted);

        if (user == null)
            return ApiResponse<LoginResponse>.Fail(400, "用户名或密码错误");

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return ApiResponse<LoginResponse>.Fail(400, "用户名或密码错误");

        var token = JwtTokenHelper.GenerateToken(user, _configuration);

        return ApiResponse<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            User = new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Gender = user.Gender,
                Phone = user.Phone,
                Email = user.Email,
            }
        });
    }

    public async Task<ApiResponse<UserProfileResponse>> GetProfileAsync(long userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new UserProfileResponse
            {
                Id = u.Id,
                Username = u.Username,
                Nickname = u.Nickname,
                Avatar = u.Avatar,
                Gender = u.Gender,
                Phone = u.Phone,
                Email = u.Email,
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return ApiResponse<UserProfileResponse>.Fail(404, "用户不存在");

        return ApiResponse<UserProfileResponse>.Success(user);
    }
}
