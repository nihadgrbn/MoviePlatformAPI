using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Movies;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MovieController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ICurrentUserService _currentUserService; 

    public MovieController(IMovieService movieService, ICurrentUserService currentUserService)
    {
        _movieService = movieService;
        _currentUserService = currentUserService;
    }

    private List<LinkDto> CreateLinksForMovie(int id)
    {
        return new List<LinkDto>
        {
            new LinkDto(Url.Link("UpdateMovie", new { id })!, "update", "PUT"),
            new LinkDto(Url.Link("DeleteMovie", new { id })!, "delete", "DELETE")
            
        };
    }
    
    private PaginationLinksDto CreatePaginationLinks(PaginationMetaDto meta, MovieQueryParametersDto parameters)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

        string CreatePageUrl(int page)
        {
            var url = $"{baseUrl}?pageNumber={page}&pageSize={parameters.PageSize}";

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                url += $"&searchTerm={parameters.SearchTerm}";

            if (parameters.Genre.HasValue)
                url += $"&genre={parameters.Genre.Value}";

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                url += $"&sortBy={parameters.SortBy}";
                
            if (parameters.IsDescending)
                url += $"&isDescending=true";

            return url;
        }

        return new PaginationLinksDto
        {
            Self = CreatePageUrl(meta.PageNumber),
            First = CreatePageUrl(1),
            Last = CreatePageUrl(meta.TotalPages == 0 ? 1 : meta.TotalPages),
            Next = meta.HasNext ? CreatePageUrl(meta.PageNumber + 1) : null,
            Previous = meta.HasPrevious ? CreatePageUrl(meta.PageNumber - 1) : null
        };
    }

    [AllowAnonymous]
    [HttpGet("genres")]
    public IActionResult GetGenres()
    {
        var genres = Enum.GetValues<Enums.MovieGenre>()
            .Select(g => new 
            { 
                Id = (int)g, 
                Name = g.ToString() 
            })
            .ToList();

        return Ok(genres);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponseDto<MovieResponseDto>>> GetMovies([FromQuery] MovieQueryParametersDto queryParameters)
    {
        var movies = await _movieService.GetAllMoviesAsync(queryParameters);
        
        var currentUserName = _currentUserService.Username;

        foreach (var movie in movies.Data)
        {
            if (!string.IsNullOrEmpty(currentUserName) && movie.OwnerUsername == currentUserName)
            {
                movie.Links = CreateLinksForMovie(movie.Id); 
            }
            else
            {
                movie.Links = null;
            }
        }

        movies.Links = CreatePaginationLinks(movies.Meta, queryParameters);

        return Ok(movies);
    }

    [HttpGet("my-movies")]
    public async Task<ActionResult<PagedResponseDto<MovieResponseDto>>> GetMyMovies([FromQuery] MovieQueryParametersDto queryParameters)
    {
        var userId = _currentUserService.UserId;
        
        var movies = await _movieService.GetMyMoviesAsync(userId, queryParameters);

        foreach (var movie in movies.Data)
        {
            movie.Links = CreateLinksForMovie(movie.Id);
        }

        movies.Links = CreatePaginationLinks(movies.Meta, queryParameters);

        return Ok(movies);
    }

    [HttpPost]
    public async Task<ActionResult<MovieResponseDto>> AddMovie(MovieCreateDto movieDto)
    {
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.Username;

        var movie = await _movieService.AddMovieAsync(movieDto, userId, userName);
        movie.Links = CreateLinksForMovie(movie.Id);

        return Ok(movie);
    }

    [HttpPut("{id}", Name = "UpdateMovie")]
    public async Task<ActionResult<MovieResponseDto>> UpdateMovie(int id, MovieCreateDto movieDto)
    {
        var userId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;

        var updatedMovie = await _movieService.UpdateMovieAsync(id, movieDto, userId, isAdmin);

        updatedMovie.Links = CreateLinksForMovie(updatedMovie.Id);
        return Ok(updatedMovie);
    }

    [HttpDelete("{id}", Name = "DeleteMovie")]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        var userId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;

        await _movieService.DeleteMovieAsync(id, userId, isAdmin);

        return Ok(new { message = "Film successfully deleted" });
    }
    
    [HttpPost("{id}/poster")]
    [Consumes("multipart/form-data")] 
    public async Task<ActionResult<MovieResponseDto>> UploadPoster(int id, IFormFile file) 
    {
        var userId = _currentUserService.UserId; 
        var isAdmin = _currentUserService.IsAdmin; 

        var result = await _movieService.UploadPosterAsync(id, file, userId, isAdmin);
        
        result.Links = CreateLinksForMovie(result.Id);
    
        return Ok(result); 
    }
    [HttpDelete("{id}/poster")]
    public async Task<ActionResult> DeletePoster(int id)
    {
        var userId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;
    
        await _movieService.DeletePosterAsync(id, userId, isAdmin);
    
        return Ok(new { message = "Poster deleted successfully" });
    }
}