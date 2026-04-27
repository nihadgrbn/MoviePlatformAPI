using MoviePlatformAPI.DTOs.Ratings;

namespace MoviePlatformAPI.Services.Contracts;

public interface IRatingService
{
    Task AddOrUpdateRatingAsync(int movieId, int userId, RatingCreateDto ratingDto);
    Task<MovieRatingStatsDto> GetMovieRatingAsync(int movieId);
}