using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs.Ratings;
using MoviePlatformAPI.Exceptions;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Services;

public class RatingService : IRatingService
{
    private readonly AppDbContext _context;

    public RatingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateRatingAsync(int movieId, int userId, RatingCreateDto ratingDto)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
        if (!movieExists)
            throw new NotFoundException("Movie not found.");

        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

        if (existingRating != null)
        {
            existingRating.Score = ratingDto.Score;
        }
        else
        {
            var rating = new Rating
            {
                MovieId = movieId,
                UserId = userId,
                Score = ratingDto.Score
            };
            _context.Ratings.Add(rating);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<MovieRatingStatsDto> GetMovieRatingAsync(int movieId)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
        if (!movieExists)
            throw new NotFoundException("Movie not found."); 
        
        var ratingsQuery = _context.Ratings.Where(r => r.MovieId == movieId);
        int totalRatings = await ratingsQuery.CountAsync();
        double averageRating = totalRatings > 0 ? await ratingsQuery.AverageAsync(r => r.Score) : 0;
        return new MovieRatingStatsDto
        {
            AverageRating = Math.Round(averageRating, 1), 
            TotalRating = totalRatings
        };
    }
}