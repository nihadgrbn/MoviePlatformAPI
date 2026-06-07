using MoviePlatformAPI.Enums;

namespace MoviePlatformAPI.Services.Contracts;

public interface ICurrentUserService
{
    int UserId { get; }
    string Username { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
    bool IsModerator { get; }
}