using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.Enums;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet("users")]
    public async Task<ActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var totalUsers = await _context.Users.CountAsync();
        var users = await _context.Users
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                Role = u.Role.ToString(),
                MoviesCount = u.Movies.Count,
                CommentsCount = u.Comments.Count
            })
            .ToListAsync();

        return Ok(new
        {
            TotalUsers = totalUsers,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = users
        });
    }


    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult> ChangeUserRole(int userId, [FromBody] ChangeRoleRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        if (!Enum.IsDefined(typeof(UserRole), request.NewRole))
            return BadRequest(new { message = "Invalid role specified." });

        var oldRole = user.Role;
        user.Role = request.NewRole;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User role updated successfully.",
            UserId = userId,
            Username = user.Username,
            OldRole = oldRole.ToString(),
            NewRole = user.Role.ToString()
        });
    }

  
    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Movies)
            .Include(u => u.Comments)
            .Include(u => u.Ratings)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"User '{user.Username}' and all related data have been deleted." });
    }

  
    [HttpGet("statistics")]
    public async Task<ActionResult> GetStatistics()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalMovies = await _context.Movies.CountAsync();
        var totalComments = await _context.Comments.CountAsync();
        var totalRatings = await _context.Ratings.CountAsync();

        var usersByRole = await _context.Users
            .GroupBy(u => u.Role)
            .Select(g => new
            {
                Role = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalMovies = totalMovies,
            TotalComments = totalComments,
            TotalRatings = totalRatings,
            UsersByRole = usersByRole
        });
    }
}

public class ChangeRoleRequest
{
    public UserRole NewRole { get; set; }
}
