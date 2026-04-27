using Microsoft.EntityFrameworkCore;
using MoviePlatformAPI.Data;
using MoviePlatformAPI.DTOs.Comments;
using MoviePlatformAPI.Models;
using MoviePlatformAPI.Services.Contracts;
using MoviePlatformAPI.Exceptions;

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

        var comment = new Comment
        {
            MovieId = movieId,
            UserId = userId,
            Text = commentDto.Text,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentResponseDto
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            AuthorUsername = username,
            AuthorId = userId 
        };
    }
    
    public async Task<CommentResponseDto> UpdateCommentAsync(int id, CommentUpdateDto updateDto, int userId)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (comment == null)
            throw new NotFoundException("Comment not found.");
            
        if (comment.UserId != userId)
        {
            throw new UnauthorizedException("You are not authorized to update this comment.");  
        }
        
        comment.Text = updateDto.Text;
        await _context.SaveChangesAsync();
        
        return new CommentResponseDto
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            AuthorUsername = comment.User!.Username,
            AuthorId = comment.UserId
        };
    }

    public async Task<List<CommentResponseDto>> GetCommentsAsync(int movieId)
    {
        return await _context.Comments
            .Include(c => c.User) 
            .Where(c => c.MovieId == movieId)
            .OrderBy(c => c.CreatedAt) 
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                AuthorUsername = c.User!.Username,
                AuthorId = c.UserId
            })
            .ToListAsync();
    }

    public async Task DeleteCommentAsync(int commentId, int userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        
        if (comment == null)
            throw new NotFoundException("Comment not found.");

        if (comment.UserId != userId)
        {
            throw new UnauthorizedException("You can only delete your own comments.");
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        
    }
}