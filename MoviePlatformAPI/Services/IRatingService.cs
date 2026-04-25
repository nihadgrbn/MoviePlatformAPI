using MoviePlatformAPI.DTOs.Ratings;

namespace MoviePlatformAPI.Services;

public interface IRatingService
{
    Task AddOrUpdateRatingAsync(int movieId, int userId, RatingCreateDto ratingDto);
    Task<MovieRatingStatsDto> GetMovieRatingAsync(int movieId);
}