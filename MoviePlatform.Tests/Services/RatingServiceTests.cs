using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.DTOs.Ratings;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.Exceptions;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services;

namespace MoviePlatformAPI.Tests.Services;

public class RatingServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetMovieRatingAsync_WhenMovieDoesNotExist_ThrowsNotFoundException()
    {
        var context = GetInMemoryDbContext();
        var ratingService = new RatingService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => ratingService.GetMovieRatingAsync(999));
    }

    [Fact]
    public async Task GetMovieRatingAsync_WhenMovieHasNoRatings_ReturnsZero()
    {
        var context = GetInMemoryDbContext();

        context.Movies.Add(new Movie { Id = 1, Title = "Test Movie" });
        await context.SaveChangesAsync();

        var ratingService = new RatingService(context);

        var result = await ratingService.GetMovieRatingAsync(1);

        Assert.Equal(0, result.AverageRating);
        Assert.Equal(0, result.TotalRating);
    }

    [Fact]
    public async Task AddOrUpdateRatingAsync_WhenRatingDoesNotExist_AddsNewRating()
    {
        var context = GetInMemoryDbContext();
        context.Movies.Add(new Movie { Id = 1, Title = "Test Movie" });
        await context.SaveChangesAsync();

        var ratingService = new RatingService(context);

        await ratingService.AddOrUpdateRatingAsync(1, 10, new RatingCreateDto { Score = 5 });

        var rating = await context.Ratings.SingleAsync();
        Assert.Equal(1, rating.MovieId);
        Assert.Equal(10, rating.UserId);
        Assert.Equal(5, rating.Score);
    }

    [Fact]
    public async Task AddOrUpdateRatingAsync_WhenRatingExists_UpdatesExistingRating()
    {
        var context = GetInMemoryDbContext();
        context.Movies.Add(new Movie { Id = 1, Title = "Test Movie" });
        context.Ratings.Add(new Rating { Id = 1, MovieId = 1, UserId = 10, Score = 2 });
        await context.SaveChangesAsync();

        var ratingService = new RatingService(context);

        await ratingService.AddOrUpdateRatingAsync(1, 10, new RatingCreateDto { Score = 4 });

        var rating = await context.Ratings.SingleAsync(r => r.MovieId == 1 && r.UserId == 10);
        Assert.Equal(4, rating.Score);
        Assert.Equal(1, await context.Ratings.CountAsync());
    }

    [Fact]
    public async Task GetMovieRatingAsync_WhenMovieHasRatings_ReturnsAverageAndTotal()
    {
        var context = GetInMemoryDbContext();
        context.Movies.Add(new Movie { Id = 1, Title = "Test Movie" });
        context.Ratings.AddRange(
            new Rating { MovieId = 1, UserId = 10, Score = 4 },
            new Rating { MovieId = 1, UserId = 11, Score = 5 }
        );
        await context.SaveChangesAsync();

        var ratingService = new RatingService(context);

        var result = await ratingService.GetMovieRatingAsync(1);

        Assert.Equal(4.5, result.AverageRating);
        Assert.Equal(2, result.TotalRating);
    }
}