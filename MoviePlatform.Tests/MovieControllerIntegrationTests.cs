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
}