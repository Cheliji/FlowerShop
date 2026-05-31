using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;

namespace FlowerShop.Api.Services;

public interface IAuthService
{
    Task<ApiResponse<object>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserProfileResponse>> GetProfileAsync(long userId);
}
