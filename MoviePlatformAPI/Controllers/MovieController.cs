using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Movies;
using MoviePlatformAPI.DTOs.Shared;
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
            new LinkDto(Url.Link("UpdateMovie", new { id })!, "update_movie", "PUT"),
            new LinkDto(Url.Link("DeleteMovie", new { id })!, "delete_movie", "DELETE")
        };
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponseDto<MovieResponseDto>>> GetMovies([FromQuery] MovieQueryParametersDto queryParameters)
    {
        var movies = await _movieService.GetAllMoviesAsync(queryParameters);
        
        foreach (var movie in movies.Data)
        {
            movie.Links = CreateLinksForMovie(movie.Id);
        }

        return Ok(movies);
    }

    [HttpGet("my-movies")]
    public async Task<ActionResult<PagedResponseDto<MovieResponseDto>>> GetMyMovies([FromQuery] MovieQueryParametersDto queryParameters)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Name)!;

        var movie = await _movieService.AddMovieAsync(movieDto, userId, userName);
        movie.Links = CreateLinksForMovie(movie.Id);

        return Ok(movie);
    }

    [HttpPut("{id}", Name = "UpdateMovie")]
    public async Task<ActionResult<MovieResponseDto>> UpdateMovie(int id, MovieCreateDto movieDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedMovie = await _movieService.UpdateMovieAsync(id, movieDto, userId);

        if (updatedMovie == null) return NotFound("Movie not found.");

        updatedMovie.Links = CreateLinksForMovie(updatedMovie.Id);
        return Ok(updatedMovie);
    }

    [HttpDelete("{id}", Name = "DeleteMovie")]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isDeleted = await _movieService.DeleteMovieAsync(id, userId);

        if (!isDeleted) return NotFound("Movie not found.");

        return Ok(new { message = "Film successfully deleted" });
    }




}