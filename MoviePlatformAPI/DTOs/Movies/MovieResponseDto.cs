using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Enums;

namespace MoviePlatformAPI.DTOs.Movies;

public class MovieResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public MovieGenre Genre { get; set; }    
    public string OwnerUsername { get; set; } = string.Empty;
    public List<LinkDto>? Links { get; set; } = new List<LinkDto>();
}