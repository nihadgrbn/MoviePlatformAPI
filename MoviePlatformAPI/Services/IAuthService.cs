using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Services;

public interface IAuthService
{
    Task<User?> Register(UserRegisterDto request);
    Task<string?> Login(UserLoginDto request);
}