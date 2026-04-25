using MoviePlatformAPI.DTOs.Shared;

namespace MoviePlatformAPI.DTOs.Movies;

public class MovieResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    
    public string OwnerUsername { get; set; } = string.Empty;
    public List<LinkDto> Links { get; set; } = new List<LinkDto>();
}