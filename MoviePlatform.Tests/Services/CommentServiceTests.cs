using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.Exceptions;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services;

namespace MoviePlatformAPI.Tests.Services;

public class CommentServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetCommentsAsync_WhenMovieDoesNotExist_ThrowsNotFoundException()
    {
        var context = GetInMemoryDbContext();
        var service = new CommentService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetCommentsAsync(999));
    }

    [Fact]
    public async Task GetCommentsAsync_ReturnsComments_InChronologicalOrder()
    {
        var context = GetInMemoryDbContext();

        context.Users.AddRange(
            new User { Id = 1, Username = "user1", Email = "user1@test.com", PasswordHash = "hash" },
            new User { Id = 2, Username = "user2", Email = "user2@test.com", PasswordHash = "hash" }
        );
        context.Movies.Add(new Movie { Id = 1, Title = "Test Movie", UserId = 1 });
        context.Comments.AddRange(
            new Comment { Id = 1, MovieId = 1, UserId = 1, Text = "Old", CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Comment { Id = 2, MovieId = 1, UserId = 2, Text = "New", CreatedAt = new DateTime(2024, 1, 1, 11, 0, 0, DateTimeKind.Utc) }
        );
        await context.SaveChangesAsync();

        var service = new CommentService(context);

        var result = await service.GetCommentsAsync(1);

        Assert.Equal(new[] { "Old", "New" }, result.Select(x => x.Text).ToArray());
    }
}