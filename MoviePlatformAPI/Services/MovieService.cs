using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; 
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs.Movies;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services.Contracts;
using MoviePlatformAPI.Exceptions;

namespace MoviePlatformAPI.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _memoryCache; 
    private readonly ILogger<MovieService> _logger; 
    private readonly IFileService _fileService;

    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public MovieService(AppDbContext context, IMemoryCache memoryCache, ILogger<MovieService> logger, IFileService fileService)
    {
        _context = context;
        _memoryCache = memoryCache;
        _logger = logger;
        _fileService = fileService;
        
    }

    private string GetMovieCacheVersion()
    {
        if (!_memoryCache.TryGetValue("movie_cache_version", out string? version) || string.IsNullOrEmpty(version))
        {
            version = Guid.NewGuid().ToString();
            _memoryCache.Set("movie_cache_version", version); 
        }
        return version;
    }

    private void InvalidateMovieCache()
    {
        _memoryCache.Set("movie_cache_version", Guid.NewGuid().ToString());
    }

    public async Task<PagedResponseDto<MovieResponseDto>> GetAllMoviesAsync(MovieQueryParametersDto queryParameters)
    {
        string currentVersion = GetMovieCacheVersion(); 
        string searchHash = GenerateSearchHash(queryParameters.SearchTerm);
        
        string cacheKey = $"all_movies_v{currentVersion}_p{queryParameters.PageNumber}_s{queryParameters.PageSize}_g{queryParameters.Genre}_h{searchHash}_sort{queryParameters.SortBy}_{queryParameters.IsDescending}";

        if (_memoryCache.TryGetValue(cacheKey, out PagedResponseDto<MovieResponseDto>? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        bool isLocked = await _semaphore.WaitAsync(TimeSpan.FromSeconds(3));
        
        try
        {
            if (isLocked)
            {
                if (_memoryCache.TryGetValue(cacheKey, out cachedResult) && cachedResult != null)
                {
                    return cachedResult;
                }
            }

            var baseQuery = _context.Movies.AsNoTracking().Include(m => m.Owner).AsQueryable(); 
            var result = await CreatePagedResponseAsync(baseQuery, queryParameters);

            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        finally
        {
            if (isLocked) _semaphore.Release();
        }
    }

    public async Task<PagedResponseDto<MovieResponseDto>> GetMyMoviesAsync(int userId, MovieQueryParametersDto queryParameters)
    {
        string currentVersion = GetMovieCacheVersion(); 
        string searchHash = GenerateSearchHash(queryParameters.SearchTerm);
        
        string cacheKey = $"my_movies_u{userId}_v{currentVersion}_p{queryParameters.PageNumber}_s{queryParameters.PageSize}_g{queryParameters.Genre}_h{searchHash}_sort{queryParameters.SortBy}_{queryParameters.IsDescending}";

        if (_memoryCache.TryGetValue(cacheKey, out PagedResponseDto<MovieResponseDto>? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        bool isLocked = await _semaphore.WaitAsync(TimeSpan.FromSeconds(3));

        try
        {
            if (isLocked)
            {
                if (_memoryCache.TryGetValue(cacheKey, out cachedResult) && cachedResult != null)
                {
                    return cachedResult;
                }
            }

            var baseQuery = _context.Movies.AsNoTracking().Include(m => m.Owner).Where(m => m.UserId == userId).AsQueryable();
            var result = await CreatePagedResponseAsync(baseQuery, queryParameters);

            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        finally
        {
            if (isLocked) _semaphore.Release();
        }
    }
    
    public async Task<MovieResponseDto> UpdateMovieAsync(int id, MovieCreateDto movieDto, int userId, bool isAdmin)
    {
        var movie = await _context.Movies.Include(m => m.Owner).FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null) throw new NotFoundException();

        if (!isAdmin && movie.UserId != userId)
            throw new UnauthorizedException("You are not authorized to update this movie.");

        movie.Title = movieDto.Title;
        movie.Description = movieDto.Description;
        movie.ReleaseYear = movieDto.ReleaseYear;
        movie.Genre = movieDto.Genre;

        await _context.SaveChangesAsync();
        InvalidateMovieCache();

        return new MovieResponseDto
        {
            Id = movie.Id, Title = movie.Title, Description = movie.Description,
            ReleaseYear = movie.ReleaseYear, Genre = movie.Genre, OwnerUsername = movie.Owner!.Username
        };
    }

    public async Task DeleteMovieAsync(int id, int userId, bool isAdmin)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null) throw new NotFoundException("Movie not found.");

        if (!isAdmin && movie.UserId != userId)
            throw new UnauthorizedException("You are not authorized to delete this movie.");

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        InvalidateMovieCache();
    }

    public async Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId, string ownerUsername)
    {
        var movie = new Movie
        {
            Title = movieDto.Title, Description = movieDto.Description,
            ReleaseYear = movieDto.ReleaseYear, Genre = movieDto.Genre, UserId = userId 
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(); 
        InvalidateMovieCache(); 

        return new MovieResponseDto
        {
            Id = movie.Id, Title = movie.Title, Description = movie.Description,
            ReleaseYear = movie.ReleaseYear, Genre = movie.Genre, OwnerUsername = ownerUsername 
        };
    }
    
    private static string GenerateSearchHash(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) 
            return "none";

        var bytes = System.Text.Encoding.UTF8.GetBytes(searchTerm.ToLower());
        var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);

        return Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
    public async Task<MovieResponseDto> UploadPosterAsync(int movieId, IFormFile file, int userId, bool isAdmin)
    {
        var movie = await _context.Movies.Include(m => m.Owner).FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null) throw new NotFoundException("Movie not found.");

        if (!isAdmin && movie.UserId != userId)
            throw new UnauthorizedException("You are not authorized to upload poster for this movie.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        string posterUrl = await _fileService.SaveFileAsync(file, allowedExtensions);

        if (!string.IsNullOrEmpty(movie.PosterPath))
        {
            _fileService.DeleteFile(movie.PosterPath);
        }

        movie.PosterPath = posterUrl;
        await _context.SaveChangesAsync();

        InvalidateMovieCache();

        return new MovieResponseDto
        {
            Id = movie.Id, 
            Title = movie.Title, 
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear, 
            Genre = movie.Genre, 
            OwnerUsername = movie.Owner!.Username,
            PosterPath = movie.PosterPath,
           
            Links = null 
        };
    }
    public async Task DeletePosterAsync(int movieId, int userId, bool isAdmin)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null) throw new NotFoundException("Movie not found.");

        if (!isAdmin && movie.UserId != userId)
            throw new UnauthorizedException("Not authorized.");

        if (!string.IsNullOrEmpty(movie.PosterPath))
        {
            _fileService.DeleteFile(movie.PosterPath); 
            movie.PosterPath = null; 
            await _context.SaveChangesAsync();
            InvalidateMovieCache();
        }
    }

    private async Task<PagedResponseDto<MovieResponseDto>> CreatePagedResponseAsync(IQueryable<Movie> query, MovieQueryParametersDto queryParameters)
    {
        if (queryParameters.Genre.HasValue)
            query = query.Where(m => m.Genre == queryParameters.Genre.Value);

        if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
        {
            query = query.Where(m => EF.Functions.ILike(m.Title, $"%{queryParameters.SearchTerm}%"));
        }

        int totalRecords = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalRecords / (double)queryParameters.PageSize);

        if (!string.IsNullOrWhiteSpace(queryParameters.SortBy))
        {
            if (queryParameters.SortBy.Equals("Year", StringComparison.OrdinalIgnoreCase))
                query = queryParameters.IsDescending ? query.OrderByDescending(m => m.ReleaseYear) : query.OrderBy(m => m.ReleaseYear);
            else if (queryParameters.SortBy.Equals("Title", StringComparison.OrdinalIgnoreCase))
                query = queryParameters.IsDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title);
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
                OwnerUsername = m.Owner!.Username,
                // 🚀 BUDUR ÇATIŞMAYAN SƏTİR:
                PosterPath = m.PosterPath 
            }).ToListAsync();
            
        var meta = new PaginationMetaDto
        {
            TotalRecords = totalRecords, TotalPages = totalPages,
            PageNumber = queryParameters.PageNumber, PageSize = queryParameters.PageSize,
            HasNext = queryParameters.PageNumber < totalPages, HasPrevious = queryParameters.PageNumber > 1
        };

        var links = new PaginationLinksDto
        {
            Self = $"/api/movies?pageNumber={queryParameters.PageNumber}&pageSize={queryParameters.PageSize}",
            Next = queryParameters.PageNumber < totalPages ? $"/api/movies?pageNumber={queryParameters.PageNumber + 1}&pageSize={queryParameters.PageSize}" : null,
            Previous = queryParameters.PageNumber > 1 ? $"/api/movies?pageNumber={queryParameters.PageNumber - 1}&pageSize={queryParameters.PageSize}" : null
        };

        return new PagedResponseDto<MovieResponseDto>
        {
            Data = movies, Meta = meta, Links = links 
        };
    }
}