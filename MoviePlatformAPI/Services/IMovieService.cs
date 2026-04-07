using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieResponseDto>> GetAllMoviesAsync();
    Task<bool> DeleteMovieAsync(int id, int userId);
    Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId , string ownerUsername);
    Task<MovieResponseDto?> UpdateMovieAsync(int id, MovieCreateDto movieDto, int userId);
    Task<IEnumerable<MovieResponseDto>> GetMyMoviesAsync(int userId);
}