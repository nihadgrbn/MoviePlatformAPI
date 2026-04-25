using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Ratings;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Services;

namespace MoviePlatformAPI.Controllers;

[Route("api/Movie")] 
[ApiController]
[Authorize]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost("{movieId}/ratings")]
    public async Task<ActionResult> AddOrUpdateRating(int movieId, [FromBody] RatingCreateDto ratingDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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