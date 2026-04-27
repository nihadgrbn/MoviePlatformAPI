using MoviePlatformAPI.DTOs.Comments;

namespace MoviePlatformAPI.Services.Contracts;

public interface ICommentService
{
    Task<CommentResponseDto> AddCommentAsync(int movieId, int userId, string username, CommentCreateDto commentDto);
    Task<List<CommentResponseDto>> GetCommentsAsync(int movieId);
    Task<CommentResponseDto> UpdateCommentAsync(int id, CommentUpdateDto updateDto, int userId);
    Task DeleteCommentAsync(int commentId, int userId);
}