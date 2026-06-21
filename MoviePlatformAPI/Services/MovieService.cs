using Elastic.Clients.Elasticsearch;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs.Movies;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Exceptions;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services.Contracts;
using System.Text.Json;

namespace MoviePlatformAPI.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MovieService> _logger;
    private readonly IFileService _fileService;
    private readonly ElasticsearchClient _elasticClient;

    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public MovieService(AppDbContext context, IDistributedCache cache, ILogger<MovieService> logger, IFileService fileService, ElasticsearchClient elasticClient)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _fileService = fileService;
        _elasticClient = elasticClient;
    }

    private async Task<string> GetMovieCacheVersionAsync()
    {
        var version = await _cache.GetStringAsync("movie_cache_version");
        if (string.IsNullOrEmpty(version))
        {
            version = Guid.NewGuid().ToString();
            await _cache.SetStringAsync("movie_cache_version", version);
        }
        return version;
    }

    private async Task InvalidateMovieCacheAsync()
    {
        await _cache.SetStringAsync("movie_cache_version", Guid.NewGuid().ToString());
    }

    public async Task<PagedResponseDto<MovieResponseDto>> GetAllMoviesAsync(MovieQueryParametersDto queryParameters)
    {
        string currentVersion = await GetMovieCacheVersionAsync();
        string searchHash = GenerateSearchHash(queryParameters.SearchTerm);

        string cacheKey = $"all_movies_v{currentVersion}_p{queryParameters.PageNumber}_s{queryParameters.PageSize}_g{queryParameters.Genre}_h{searchHash}_sort{queryParameters.SortBy}_{queryParameters.IsDescending}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<PagedResponseDto<MovieResponseDto>>(cachedData)!;
        }

        bool isLocked = await _semaphore.WaitAsync(TimeSpan.FromSeconds(3));

        try
        {
            if (isLocked)
            {
                cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<PagedResponseDto<MovieResponseDto>>(cachedData)!;
                }
            }

            var baseQuery = _context.Movies.AsNoTracking().Include(m => m.Owner).AsQueryable();
            var result = await CreatePagedResponseAsync(baseQuery, queryParameters);

            var serializedResult = JsonSerializer.Serialize(result);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions);

            return result;
        }
        finally
        {
            if (isLocked) _semaphore.Release();
        }
    }

    public async Task<PagedResponseDto<MovieResponseDto>> GetMyMoviesAsync(int userId, MovieQueryParametersDto queryParameters)
    {
        string currentVersion = await GetMovieCacheVersionAsync();
        string searchHash = GenerateSearchHash(queryParameters.SearchTerm);

        string cacheKey = $"my_movies_u{userId}_v{currentVersion}_p{queryParameters.PageNumber}_s{queryParameters.PageSize}_g{queryParameters.Genre}_h{searchHash}_sort{queryParameters.SortBy}_{queryParameters.IsDescending}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<PagedResponseDto<MovieResponseDto>>(cachedData)!;
        }

        bool isLocked = await _semaphore.WaitAsync(TimeSpan.FromSeconds(3));

        try
        {
            if (isLocked)
            {
                cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<PagedResponseDto<MovieResponseDto>>(cachedData)!;
                }
            }

            var baseQuery = _context.Movies.AsNoTracking().Include(m => m.Owner).Where(m => m.UserId == userId).AsQueryable();
            var result = await CreatePagedResponseAsync(baseQuery, queryParameters);

            var serializedResult = JsonSerializer.Serialize(result);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions);

            return result;
        }
        finally
        {
            if (isLocked) _semaphore.Release();
        }
    }

    public async Task<MovieResponseDto> UpdateMovieAsync(int id, MovieUpdateDto movieDto, int userId, bool isAdmin)
    {
        var movie = await _context.Movies.Include(m => m.Owner).FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null) throw new NotFoundException();

        if (!isAdmin && movie.UserId != userId)
            throw new UnauthorizedException("You are not authorized to update this movie.");

        movie.Title = movieDto.Title.Trim();
        movie.Description = movieDto.Description.Trim();
        movie.ReleaseYear = movieDto.ReleaseYear;
        movie.Genre = movieDto.Genre;

        await _context.SaveChangesAsync();
        await InvalidateMovieCacheAsync();

        return movie.Adapt<MovieResponseDto>();
    }

    public async Task DeleteMovieAsync(int id, int userId, bool isAdmin)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null) throw new NotFoundException("Movie not found.");

        if (!isAdmin && movie.UserId != userId)
            throw new UnauthorizedException("You are not authorized to delete this movie.");

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        await InvalidateMovieCacheAsync();
    }

    public async Task<MovieResponseDto> AddMovieAsync(MovieCreateDto movieDto, int userId, string ownerUsername)
    {
        movieDto.Title = movieDto.Title.Trim();
        movieDto.Description = movieDto.Description.Trim();
        var movie = movieDto.Adapt<Movie>();
        movie.UserId = userId;

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        await _elasticClient.IndexAsync(movie, i => i.Index("movies"));
        await InvalidateMovieCacheAsync();

        var response = movie.Adapt<MovieResponseDto>();
        response.OwnerUsername = ownerUsername;
        return response;
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

        await InvalidateMovieCacheAsync();

        return movie.Adapt<MovieResponseDto>();
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
            await InvalidateMovieCacheAsync();
        }
    }

    public async Task<PagedResponseDto<MovieResponseDto>> SearchMoviesAsync(MovieQueryParametersDto queryParameters)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
            {
                return BuildPagedResponse(new List<MovieResponseDto>(), 0, queryParameters);
            }

            int pageNumber = queryParameters.PageNumber < 1 ? 1 : queryParameters.PageNumber;
            int pageSize = queryParameters.PageSize < 1 ? 10 : queryParameters.PageSize;

            var searchResponse = await _elasticClient.SearchAsync<Movie>(s => s
                .Index("movies")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.Match(m => m.Field(f => f.Title).Query(queryParameters.SearchTerm).Boost(2.0f)), 
                            sh => sh.Match(m => m.Field(f => f.Description).Query(queryParameters.SearchTerm).Fuzziness(new Fuzziness(1)))   
                        )
                    )
                )
                .From((pageNumber - 1) * pageSize)
                .Size(pageSize)
            );

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogError($"ES Xətası: {searchResponse.DebugInformation}");
                return BuildPagedResponse(new List<MovieResponseDto>(), 0, queryParameters);
            }

            var movies = searchResponse.Documents.Adapt<List<MovieResponseDto>>();

            long totalRecords = searchResponse.Total;

            return BuildPagedResponse(movies, (int)totalRecords, queryParameters);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Axtarış zamanı xəta: {ex.Message}");
            return BuildPagedResponse(new List<MovieResponseDto>(), 0, queryParameters);
        }
    }

    private async Task<PagedResponseDto<MovieResponseDto>> CreatePagedResponseAsync(IQueryable<Movie> query, MovieQueryParametersDto queryParameters)
    {
        if (queryParameters.Genre.HasValue)
            query = query.Where(m => m.Genre == queryParameters.Genre.Value);

        int totalRecords = await query.CountAsync();

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
            .ProjectToType<MovieResponseDto>()
            .ToListAsync();

        return BuildPagedResponse(movies, totalRecords, queryParameters);
    }
    public async Task SyncMoviesToElasticsearchAsync()
    {
        var movies = await _context.Movies.AsNoTracking().ToListAsync();

        if (movies.Any())
        {
            var bulkResponse = await _elasticClient.IndexManyAsync(movies, "movies");

            if (!bulkResponse.IsSuccess())
            {
                _logger.LogError("Elasticsearch sinxronizasiyası zamanı xəta baş verdi.");
                throw new Exception("Filmləri Elasticsearch-ə yükləmək mümkün olmadı.");
            }
        }
    }
    private PagedResponseDto<MovieResponseDto> BuildPagedResponse(List<MovieResponseDto> data, int totalRecords, MovieQueryParametersDto queryParameters)
    {
        int totalPages = (int)Math.Ceiling(totalRecords / (double)queryParameters.PageSize);

        var meta = new PaginationMetaDto
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize,
            HasNext = queryParameters.PageNumber < totalPages,
            HasPrevious = queryParameters.PageNumber > 1
        };

        var links = new PaginationLinksDto
        {
            Self = $"/api/movies?pageNumber={queryParameters.PageNumber}&pageSize={queryParameters.PageSize}",
            Next = queryParameters.PageNumber < totalPages ? $"/api/movies?pageNumber={queryParameters.PageNumber + 1}&pageSize={queryParameters.PageSize}" : null,
            Previous = queryParameters.PageNumber > 1 ? $"/api/movies?pageNumber={queryParameters.PageNumber - 1}&pageSize={queryParameters.PageSize}" : null
        };

        return new PagedResponseDto<MovieResponseDto>
        {
            Data = data,
            Meta = meta,
            Links = links
        };
    }
}