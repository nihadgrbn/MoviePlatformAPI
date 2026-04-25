using MoviePlatformAPI.DTOs.Shared;

namespace MoviePlatformAPI.DTOs.Ratings;

public class MovieRatingStatsDto
{
    public double AverageRating { get; set; }
    public int TotalRating { get; set; }
    public List<LinkDto> Links { get; set; } = new List<LinkDto>();
}