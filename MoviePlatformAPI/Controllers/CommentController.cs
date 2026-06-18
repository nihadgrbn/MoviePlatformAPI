using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviePlatformAPI.DTOs.Comments;
using MoviePlatformAPI.DTOs.Shared;
using MoviePlatformAPI.Services.Contracts;
using Microsoft.AspNetCore.RateLimiting;

namespace MoviePlatformAPI.Controllers;

[Route("api/[controller]")] 
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ICurrentUserService _currentUserService; 
    public CommentController(ICommentService commentService, ICurrentUserService currentUserService)
    {
        _commentService = commentService;
        _currentUserService = currentUserService;
    }
    private List<LinkDto> CreateLinksForComment(int id)
    {
        return new List<LinkDto>
        {
            new LinkDto(Url.Link("UpdateComment", new { id })!, "update", "PUT"),
            new LinkDto(Url.Link("DeleteComment", new { id })!, "delete", "DELETE")
        };
    }
    [EnableRateLimiting("comment-policy")]

    [HttpPost("{movieId}/comments")]
    public async Task<ActionResult<CommentResponseDto>> AddComment(int movieId, [FromBody] CommentCreateDto commentDto)
    {
        var userId = _currentUserService.UserId;
        var username = _currentUserService.Username;

        var comment = await _commentService.AddCommentAsync(movieId, userId, commentDto);
        return Ok(comment);
    }
    [HttpPut("{id}", Name = "UpdateComment")]
    public async Task<ActionResult<CommentResponseDto>> UpdateComment(int id, [FromBody] CommentUpdateDto updateDto)
    {
        var userId = _currentUserService.UserId;
        var isModerator = _currentUserService.IsModerator;

        var updatedComment = await _commentService.UpdateCommentAsync(id, updateDto, userId, isModerator);

        updatedComment.Links = CreateLinksForComment(updatedComment.Id);

        return Ok(updatedComment);
    }
    [HttpGet("{movieId}/comments", Name = "GetComments")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CommentResponseDto>>> GetComments(int movieId)
    {
        var comments = await _commentService.GetCommentsAsync(movieId);

        var currentUserId = _currentUserService.UserId;
        var isModerator = _currentUserService.IsModerator;

        foreach (var comment in comments)
        {
            if (currentUserId != 0 && (isModerator || currentUserId == comment.AuthorId))
            {
                comment.Links = CreateLinksForComment(comment.Id);
            }
            else
            {
                comment.Links = null;
            }
        }

        return Ok(comments);
    }
    [HttpDelete("{id}", Name = "DeleteComment")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        var userId = _currentUserService.UserId;
        var isModerator = _currentUserService.IsModerator;

        await _commentService.DeleteCommentAsync(id, userId, isModerator);
        return NoContent();
    }

}