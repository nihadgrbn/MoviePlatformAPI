using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace MoviePlatform.Tests;

public class MovieControllerIntegrationTests 
{
    [Fact] 
    public async Task GetMyMovies_WithoutToken_ReturnsUnauthorized()
    {
        var factory = new WebApplicationFactory<Program>(); 
        var client = factory.CreateClient(); 

        var response = await client.GetAsync("/api/Movie/my-movies");


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task CreateMovie_WithoutToken_ReturnsUnauthorized()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var content = new StringContent("{\"title\":\"New Film\"}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/Movie", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMovie_WithInvalidRoute_ReturnsNotFound()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/Movie/random-point/123");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var badRequestContent = new StringContent(
            "{\"username\":\"TestUser\", \"email\":\"nnn@test.com\", \"password\":\"12\"}", 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await client.PostAsync("/api/Auth/register", badRequestContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task AddComment_WithoutToken_ReturnsUnauthorized()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var content = new StringContent("{\"text\":\"elede yaxs deyil\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/Comment/1/comments", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_WithoutToken_ReturnsUnauthorized()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/Comment/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddRating_WithoutToken_ReturnsUnauthorized()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var content = new StringContent("{\"score\": 5}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/Rating/1/ratings", content);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_WithoutToken_ReturnsSuccess()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/Comment/1/comments");
        response.EnsureSuccessStatusCode(); 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRatings_WithoutToken_ReturnsSuccess()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/Rating/1/ratings");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}