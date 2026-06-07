using MoviePlatformAPI.Enums;

namespace MoviePlatformAPI.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public int ReleaseYear { get; set; }
    public MovieGenre Genre { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public int UserId { get; set; }
    public User? Owner { get; set; } 
    public string? PosterPath { get; set; }
}