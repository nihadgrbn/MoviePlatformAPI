using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Ratings;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly ICurrentUserService _currentUserService; 

    public RatingController(IRatingService ratingService, ICurrentUserService currentUserService)
    {
        _ratingService = ratingService;
        _currentUserService = currentUserService;
    }

    [HttpPost("{movieId}/ratings")]
    public async Task<ActionResult> AddOrUpdateRating(int movieId, [FromBody] RatingCreateDto ratingDto)
    {
        var userId = _currentUserService.UserId;
        
        await _ratingService.AddOrUpdateRatingAsync(movieId, userId, ratingDto);
        
        return Ok(new { message = "Rating submitted successfully." });
    }

    [HttpGet("{movieId}/ratings")]
    [AllowAnonymous] 
    public async Task<ActionResult<MovieRatingStatsDto>> GetMovieRating(int movieId)
    {
        var stats = await _ratingService.GetMovieRatingAsync(movieId);
        
        stats.Links = new List<LinkDto>
        {
            new LinkDto(Url.Action("AddOrUpdateRating", new { movieId })!, "rate_movie", "POST")
        };

        return Ok(stats);
    }
}