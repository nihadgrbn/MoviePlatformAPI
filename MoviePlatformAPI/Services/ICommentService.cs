using MoviePlatformAPI.DTOs.Comments;

namespace MoviePlatformAPI.Services;

public interface ICommentService
{
    Task<CommentResponseDto> AddCommentAsync(int movieId, int userId, string username, CommentCreateDto commentDto);
    Task<List<CommentResponseDto>> GetCommentsAsync(int movieId);
    Task<bool> DeleteCommentAsync(int commentId, int userId);
}