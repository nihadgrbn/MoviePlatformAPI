using Microsoft.EntityFrameworkCore;
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
}