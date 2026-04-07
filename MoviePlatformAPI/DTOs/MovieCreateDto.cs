using System.ComponentModel.DataAnnotations;

namespace MoviePlatformAPI.DTOs;

public class MovieCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public int ReleaseYear { get; set; }
    
    [Required]
    public string Genre { get; set; } = string.Empty;
}