namespace MoviePlatformAPI.Services.Contracts;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string[] allowedExtensions);
    void DeleteFile(string path);
}