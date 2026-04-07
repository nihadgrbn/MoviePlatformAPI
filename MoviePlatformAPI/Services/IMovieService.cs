using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieResponseDto>> GetAllMoviesAsync();
    Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId , string ownerUsername);
    Task<IEnumerable<MovieResponseDto>> GetMyMoviesAsync(int userId);
}