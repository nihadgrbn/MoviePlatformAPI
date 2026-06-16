using MoviePlatformAPI.Enums;

namespace MoviePlatformAPI.DTOs.Movies;

public class MovieUpdateDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public MovieGenre Genre { get; set; }
}