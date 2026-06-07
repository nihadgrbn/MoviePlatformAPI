using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Auth;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService; 

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        
        await _authService.Register(request);
        
        return Ok(new { Message = "Register successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var response = await _authService.Login(request);
        
        return Ok(response); 
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto request)
    {
        var response = await _authService.RefreshToken(request.RefreshToken);
        
        return Ok(response);
    }

    [Authorize] 
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = _currentUserService.UserId;
        
        await _authService.Logout(userId);

        return Ok(new { Message = "Successfully logged out." });
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        await _authService.SendPasswordResetCodeAsync(request.Email);
        
        return Ok(new { message = "Password reset code has been sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        await _authService.ResetPasswordAsync(request);
        
        return Ok(new { message = "Your password has been successfully reset." });
    }
}