using MoviePlatformAPI.DTOs.Comments;

namespace MoviePlatformAPI.Services.Contracts;

public interface ICommentService
{
    Task<CommentResponseDto> AddCommentAsync(int movieId, int userId, CommentCreateDto commentDto);
    Task<List<CommentResponseDto>> GetCommentsAsync(int movieId);
    Task<CommentResponseDto> UpdateCommentAsync(int id, CommentUpdateDto updateDto, int userId, bool isModerator);
    Task DeleteCommentAsync(int commentId, int userId, bool isModerator);
}