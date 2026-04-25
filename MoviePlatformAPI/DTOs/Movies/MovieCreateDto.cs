namespace MoviePlatformAPI.DTOs.Movies;

public class MovieCreateDto
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int ReleaseYear { get; set; }
    
    public string Genre { get; set; } = string.Empty;
}