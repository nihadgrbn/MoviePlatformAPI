using MoviePlatformAPI.DTOs.Auth;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Services.Contracts;

public interface IAuthService
{
    Task<User> Register(UserRegisterDto request);
    Task<AuthResponseDto> Login(UserLoginDto request);
    Task<AuthResponseDto> RefreshToken(string refreshToken);
    Task<bool> Logout(int userId);
    
}