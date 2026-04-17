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
        var token = await _authService.Login(request);
        if (token == null) 
            return BadRequest(new { Error = "Username or password is incorrect" });
            
        return Ok(new { Token = token });
    }
}