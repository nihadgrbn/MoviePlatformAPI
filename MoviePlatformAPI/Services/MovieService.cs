using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs;
using MoviePlatformAPI.Models;

namespace MoviePlatformAPI.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;

    public MovieService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponseDto<MovieResponseDto>> GetAllMoviesAsync(MovieQueryParametersDto queryParameters)
    {
        var baseQuery = _context.Movies.AsQueryable();
        return await CreatePagedResponseAsync(baseQuery, queryParameters);
    }

    public async Task<PagedResponseDto<MovieResponseDto>> GetMyMoviesAsync(int userId, MovieQueryParametersDto queryParameters)
    {
        var baseQuery = _context.Movies.Where(m => m.UserId == userId).AsQueryable();
        return await CreatePagedResponseAsync(baseQuery, queryParameters);
    }
    
    public async Task<MovieResponseDto?> UpdateMovieAsync(int id, MovieCreateDto movieDto, int userId)
    {
        var movie = await _context.Movies
            .Include(m => m.Owner)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (movie == null)
            return null;
            
        if (movie.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this movie.");  
        }
        
        movie.Title = movieDto.Title;
        movie.Description = movieDto.Description;
        movie.ReleaseYear = movieDto.ReleaseYear;
        movie.Genre = movieDto.Genre;
        
        await _context.SaveChangesAsync();
        
        return new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear,
            Genre = movie.Genre,
            OwnerUsername = movie.Owner!.Username
        };
    }

    public async Task<bool> DeleteMovieAsync(int id, int userId)
    {
        var movie = await _context.Movies.FindAsync(id);
        
        if (movie == null)
            return false;
            
        if (movie.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this movie."); 
        }
        
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return true;
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

    
    
    private async Task<PagedResponseDto<MovieResponseDto>> CreatePagedResponseAsync(
        IQueryable<Movie> query, 
        MovieQueryParametersDto queryParameters)
    {
        if (!string.IsNullOrWhiteSpace(queryParameters.Genre))
        {
            query = query.Where(m => m.Genre.ToLower() == queryParameters.Genre.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
        {
            query = query.Where(m => m.Title.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
        }

        int totalRecords = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(queryParameters.SortBy))
        {
            if (queryParameters.SortBy.Equals("Year", StringComparison.OrdinalIgnoreCase))
            {
                query = queryParameters.IsDescending 
                    ? query.OrderByDescending(m => m.ReleaseYear) 
                    : query.OrderBy(m => m.ReleaseYear);
            }
            else if (queryParameters.SortBy.Equals("Title", StringComparison.OrdinalIgnoreCase))
            {
                query = queryParameters.IsDescending 
                    ? query.OrderByDescending(m => m.Title) 
                    : query.OrderBy(m => m.Title);
            }
        }
        else 
        {
            query = query.OrderByDescending(m => m.Id);
        }

        var movies = await query
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
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

        return new PagedResponseDto<MovieResponseDto>
        {
            Data = movies,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize,
            TotalRecords = totalRecords
        };
    }
}