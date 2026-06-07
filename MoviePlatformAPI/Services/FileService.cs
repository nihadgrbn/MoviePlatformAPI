using MoviePlatformAPI.Exceptions;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Services;

public class FileService:IFileService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 5 * 1024 * 1024; 
    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    public async Task<string> SaveFileAsync(IFormFile file, string[] allowedExtensions)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("No file uploaded.");

        if (file.Length > MaxFileSize)
            throw new BadRequestException("File size cannot exceed 2MB.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            throw new BadRequestException("Invalid file type. Only JPG, JPEG, PNG, and WEBP are allowed.");

        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "posters");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/posters/{uniqueFileName}";
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        string fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}