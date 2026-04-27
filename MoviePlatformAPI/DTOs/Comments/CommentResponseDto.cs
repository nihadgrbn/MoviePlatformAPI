using MoviePlatformAPI.DTOs.Shared;

namespace MoviePlatformAPI.DTOs.Comments;

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public List<LinkDto>? Links { get; set; } = new List<LinkDto>();
}