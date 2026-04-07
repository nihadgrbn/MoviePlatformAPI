using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Models;
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

    [HttpGet]
    [Authorize] 
    public async Task<ActionResult<IEnumerable<MovieResponseDto>>> GetMovies()
    {
        var movies = await _movieService.GetAllMoviesAsync();
        return Ok(movies);
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteMovie(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) 
            return Unauthorized("System could not identify you. Please login again.");
        var userId = int.Parse(userIdString);
        try
        {
            var isDeleted = await _movieService.DeleteMovieAsync(id, userId);

            if (!isDeleted)
                return NotFound("“The film you want to delete was not found.”\n"); 

            return Ok("Film uğurla silindi!"); 
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message); 
        }
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<MovieResponseDto>> UpdateMovie(int id, MovieCreateDto movieDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) 
            return Unauthorized("You are not authorized to update this movie.");

        var userId = int.Parse(userIdString);

        try
        {
            var updatedMovie = await _movieService.UpdateMovieAsync(id, movieDto, userId);

            if (updatedMovie == null)
                return NotFound("Not found you wanwa update film"); // 404

            return Ok(updatedMovie); 
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message); 
        }
    }

    [HttpGet("my-movies")]
    public async Task<ActionResult<IEnumerable<MovieResponseDto>>> GetMyMovies()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) 
            return Unauthorized("System could not identify you. Please login again.");

        var userId = int.Parse(userIdString);

        var movies = await _movieService.GetMyMoviesAsync(userId);

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

        return Ok(movie);
    }
}