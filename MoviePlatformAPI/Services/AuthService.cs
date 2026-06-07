using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs.Auth;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services.Contracts;
using MoviePlatformAPI.Exceptions; 

namespace MoviePlatformAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache; 
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext context, IConfiguration configuration, IMemoryCache cache, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _cache = cache; 
        _emailService = emailService;
    }
    
    public async Task SendPasswordResetCodeAsync(string email)
    {
        string cacheKey = $"ResetPassword_{email}";

        if (_cache.TryGetValue(cacheKey, out string? existingCode) && !string.IsNullOrEmpty(existingCode))
        {
            throw new BadRequestException("A reset code has already been sent. Please wait 5 minutes before requesting a new one.", "COOLDOWN_ACTIVE");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            throw new NotFoundException("User with this email was not found.", "USER_NOT_FOUND");

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

        string subject = "MoviePlatform - Password Reset Code";
        string body = $@"
            <h2>Password Reset Request</h2>
            <p>Your password reset code is: <strong>{otpCode}</strong></p>
            <p>This code will expire in 5 minutes. If you didn't request this, just ignore this email.</p>";

        await _emailService.SendEmailAsync(email, subject, body);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto request)
    {
        string cacheKey = $"ResetPassword_{request.Email}";

        if (!_cache.TryGetValue(cacheKey, out string? savedOtpCode) || string.IsNullOrEmpty(savedOtpCode))
        {
            throw new BadRequestException("OTP code has expired or is invalid.", "OTP_EXPIRED");
        }

        if (savedOtpCode != request.OtpCode)
        {
            throw new BadRequestException("Invalid OTP code.", "INVALID_OTP");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            throw new NotFoundException("User not found.", "USER_NOT_FOUND");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        _cache.Remove(cacheKey);
    }

    public async Task<User> Register(UserRegisterDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
            throw new BadRequestException("This email is already in use.", "EMAIL_IN_USE"); 

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<AuthResponseDto> Login(UserLoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid username or password.");
            
        var token = CreateToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponseDto { Token = token, RefreshToken = refreshToken };
    }

    public async Task<AuthResponseDto> RefreshToken(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token."); 

        var newAccessToken = CreateToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _context.SaveChangesAsync();

        return new AuthResponseDto { Token = newAccessToken, RefreshToken = newRefreshToken };
    }

    public async Task<bool> Logout(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null) 
            throw new NotFoundException("User not found."); 

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _context.SaveChangesAsync();

        return true;
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), 
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber); 
    }
}