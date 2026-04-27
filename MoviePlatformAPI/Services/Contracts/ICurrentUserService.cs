namespace MoviePlatformAPI.Services.Contracts;

public interface ICurrentUserService
{
    int UserId { get; }
    string Username { get; }
}