using System.Security.Claims;
using MoviePlatformAPI.Enums;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userIdClaim) ? 0 : int.Parse(userIdClaim);
        }
    }

    public string Username
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        }
    }

    public UserRole Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return string.IsNullOrEmpty(roleClaim) ? UserRole.User : Enum.Parse<UserRole>(roleClaim);
        }
    }

    public bool IsAdmin => Role == UserRole.Admin;

    public bool IsModerator => Role == UserRole.Moderator || Role == UserRole.Admin;
}