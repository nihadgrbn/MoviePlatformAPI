using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Comments;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Services;

namespace MoviePlatformAPI.Controllers;

[Route("api/Movie")] 
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private List<LinkDto> CreateLinksForComment(int movieId, int commentId, int? currentUserId, int commentAuthorId)
    {
        var links = new List<LinkDto>
        {
            new LinkDto(Url.Link("GetComments", new { movieId })!, "self", "GET")
        };

        if (currentUserId.HasValue && currentUserId == commentAuthorId)
        {
            links.Add(new LinkDto(Url.Link("DeleteComment", new { commentId })!, "delete_comment", "DELETE"));
        }

        return links;
    }

    [HttpPost("{movieId}/comments")]
    public async Task<ActionResult<CommentResponseDto>> AddComment(int movieId, [FromBody] CommentCreateDto commentDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var username = User.FindFirstValue(ClaimTypes.Name)!;

        var comment = await _commentService.AddCommentAsync(movieId, userId, username, commentDto);
        return Ok(comment);
    }

    [HttpGet("{movieId}/comments", Name = "GetComments")]
    [AllowAnonymous] 
    public async Task<ActionResult<List<CommentResponseDto>>> GetComments(int movieId)
    {
        var comments = await _commentService.GetCommentsAsync(movieId);
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = userIdStr != null ? int.Parse(userIdStr) : null;

        foreach (var comment in comments)
        {
            comment.Links = CreateLinksForComment(movieId, comment.Id, currentUserId, comment.AuthorId); 
        }

        return Ok(comments);
    }

    [HttpDelete("comments/{commentId}", Name = "DeleteComment")]
    public async Task<ActionResult> DeleteComment(int commentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isDeleted = await _commentService.DeleteCommentAsync(commentId, userId);
        
        if (!isDeleted) return NotFound("Comment not found.");

        return NoContent(); 
    }
}