using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Services;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MovieController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MovieController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    private List<LinkDto> CreateLinksForMovie(int id)
    {
        return new List<LinkDto>
        {
            new LinkDto(Url.Link("UpdateMovie", new { id }), "update_movie", "PUT"),
            new LinkDto(Url.Link("DeleteMovie", new { id }), "delete_movie", "DELETE")
        };
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<MovieResponseDto>>> GetMovies([FromQuery] MovieQueryParametersDto queryParameters)
    {
        var movies = await _movieService.GetAllMoviesAsync(queryParameters);
        
        foreach (var movie in movies.Data)
        {
            movie.Links = CreateLinksForMovie(movie.Id);
        }

        return Ok(movies);
    }

    [HttpDelete("{id}", Name = "DeleteMovie")]
    public async Task<ActionResult<bool>> DeleteMovie(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) 
            return Unauthorized("System could not identify you. Please login again.");
            
        var userId = int.Parse(userIdString);
        
        var isDeleted = await _movieService.DeleteMovieAsync(id, userId);

        if (!isDeleted)
            return NotFound("The film you want to delete was not found."); 

        return Ok("Film successfully deleted");
    }

    [HttpPut("{id}", Name = "UpdateMovie")]
    public async Task<ActionResult<MovieResponseDto>> UpdateMovie(int id, MovieCreateDto movieDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) 
            return Unauthorized("You are not authorized to update this movie.");

        var userId = int.Parse(userIdString);

        var updatedMovie = await _movieService.UpdateMovieAsync(id, movieDto, userId);

        if (updatedMovie == null)
            return NotFound("Not found you wanna update film"); 

        updatedMovie.Links = CreateLinksForMovie(updatedMovie.Id);

        return Ok(updatedMovie);
    }

    [HttpGet("my-movies")]
    public async Task<ActionResult<PagedResponseDto<MovieResponseDto>>> GetMyMovies([FromQuery] MovieQueryParametersDto queryParameters)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) 
            return Unauthorized("System could not identify you. Please login again.");

        var userId = int.Parse(userIdString);

        var movies = await _movieService.GetMyMoviesAsync(userId, queryParameters);

        foreach (var movie in movies.Data)
        {
            movie.Links = CreateLinksForMovie(movie.Id);
        }

        return Ok(movies);
    }

    [HttpPost]
    public async Task<ActionResult<MovieResponseDto>> AddMovie(MovieCreateDto movieDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name); 
        
        if (userIdString == null || userName == null) 
            return Unauthorized("System could not identify you. Please login again.");

        var userId = int.Parse(userIdString);

        var movie = await _movieService.AddMovieAsync(movieDto, userId, userName);

        movie.Links = CreateLinksForMovie(movie.Id);

        return Ok(movie);
    }
}