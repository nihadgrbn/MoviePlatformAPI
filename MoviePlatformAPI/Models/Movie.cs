using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviePlatformAPI.Models;

public class Movie
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required] public string? Description { get; set; }
    public int ReleaseYear { get; set; }
    [Required]
    public string? Genre { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User? Owner { get; set; }    
    
    
    
}