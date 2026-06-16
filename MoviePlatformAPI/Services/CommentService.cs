using Mapster;
using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs.Comments;
using MoviePlatformAPI.Exceptions;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;

    public CommentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CommentResponseDto> AddCommentAsync(int movieId, int userId, string username, CommentCreateDto commentDto)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
        if (!movieExists)
            throw new NotFoundException("Movie not found.");
        commentDto.Text = commentDto.Text.Trim();

        var comment = commentDto.Adapt<Comment>();

        comment.MovieId = movieId;
        comment.UserId = userId;
        comment.CreatedAt = DateTime.UtcNow;

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var response = comment.Adapt<CommentResponseDto>();
        return response;
    }
    
    public async Task<CommentResponseDto> UpdateCommentAsync(int id, CommentUpdateDto updateDto, int userId, bool isModerator)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            throw new NotFoundException("Comment not found.");

        if (!isModerator && comment.UserId != userId)
        {
            throw new UnauthorizedException("You are not authorized to update this comment.");
        }

        comment.Text = updateDto.Text.Trim();
        await _context.SaveChangesAsync();

        return comment.Adapt<CommentResponseDto>();
    }

    public async Task<List<CommentResponseDto>> GetCommentsAsync(int movieId)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
        if (!movieExists)
            throw new NotFoundException("Movie not found.");

        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.MovieId == movieId)
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .ProjectToType<CommentResponseDto>()
            .ToListAsync();
    }

    public async Task DeleteCommentAsync(int commentId, int userId, bool isModerator)
    {
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found.");

        if (!isModerator && comment.UserId != userId)
        {
            throw new UnauthorizedException("You can only delete your own comments.");
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}