using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviePlatformAPI.Models;

public class Movie
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(250)] 
    public string Title { get; set; } = string.Empty;

    [Required] 
    [MaxLength(1500)] 
    public string Description { get; set; } = string.Empty; 
    
    public int ReleaseYear { get; set; }
    
    [Required]
    [MaxLength(50)] 
    public string Genre { get; set; } = string.Empty; 
    
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? Owner { get; set; } 
}