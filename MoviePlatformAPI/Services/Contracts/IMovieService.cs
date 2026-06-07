using MoviePlatformAPI.DTOs.Movies;
using MoviePlatformAPI.DTOs.Shared;

namespace MoviePlatformAPI.Services.Contracts;

public interface IMovieService
{
    Task<PagedResponseDto<MovieResponseDto>> GetAllMoviesAsync(MovieQueryParametersDto queryParameters);
    Task<PagedResponseDto<MovieResponseDto>> GetMyMoviesAsync(int userId, MovieQueryParametersDto queryParameters);
    Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId, string ownerUsername);
    Task<MovieResponseDto> UpdateMovieAsync(int id, MovieCreateDto movieDto, int userId, bool isAdmin);
    Task DeleteMovieAsync(int id, int userId, bool isAdmin);
    Task<MovieResponseDto> UploadPosterAsync(int movieId, IFormFile file, int userId, bool isAdmin);
    Task DeletePosterAsync(int movieId, int userId, bool isAdmin);
}