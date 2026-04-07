using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Services;

public class MovieService:IMovieService
{
    private readonly AppDbContext _context;

    public MovieService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<MovieResponseDto>> GetAllMoviesAsync()
    {
        return await _context.Movies
            .Include(m => m.Owner) 
            .Select(m => new MovieResponseDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                Genre = m.Genre,
                OwnerUsername = m.Owner!.Username 
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<MovieResponseDto>> GetMyMoviesAsync(int userId)
    {
        return await _context.Movies
            .Where(m => m.UserId == userId) 
            .Include(m => m.Owner)          
            .Select(m => new MovieResponseDto 
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                Genre = m.Genre,
                OwnerUsername = m.Owner!.Username 
            })
            .ToListAsync();
    }

    public async Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId, string ownerUsername)
    {
        var movie = new Movie
        {
            Title = movieDto.Title,
            Description = movieDto.Description,
            ReleaseYear = movieDto.ReleaseYear,
            Genre = movieDto.Genre,
            UserId = userId 
        };

        _context.Movies.Add(movie);
    
        await _context.SaveChangesAsync(); 

        return new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear,
            Genre = movie.Genre,
            OwnerUsername = ownerUsername 
        };
    }
}