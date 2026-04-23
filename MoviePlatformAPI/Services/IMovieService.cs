using MoviePlatformAPI.DTOs;

namespace MoviePlatformAPI.Services;

public interface IMovieService
{
    Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId, string ownerUsername);
    Task<MovieResponseDto?> UpdateMovieAsync(int id, MovieCreateDto movieDto, int userId);
    Task<bool> DeleteMovieAsync(int id, int userId);

    Task<PagedResponseDto<MovieResponseDto>> GetAllMoviesAsync(MovieQueryParametersDto queryParameters);
    Task<PagedResponseDto<MovieResponseDto>> GetMyMoviesAsync(int userId, MovieQueryParametersDto queryParameters);
}