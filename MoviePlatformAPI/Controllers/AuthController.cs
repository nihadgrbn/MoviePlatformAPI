using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Services;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        var result = await _authService.Register(request);
        if (result == null) 
            return BadRequest(new { Error = "Username or email already exists." });
            
        return Ok(new { Message = "Register successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var response = await _authService.Login(request);
        if (response == null) 
            return BadRequest(new { Error = "Username or password is incorrect" });
            
        return Ok(response); 
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto request)
    {
        var response = await _authService.RefreshToken(request.RefreshToken);
        if (response == null)
            return Unauthorized(new { Error = "Invalid or expired refresh token." });

        return Ok(response);
    }

    [Authorize] 
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(userIdString, out int userId))
            return Unauthorized(); 

        var result = await _authService.Logout(userId);
        if (!result)
            return BadRequest(new { Error = "User not found" });

        return Ok(new { Message = "Successfully logged out." });
    }
}