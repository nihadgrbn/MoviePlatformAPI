using System.ComponentModel.DataAnnotations;

namespace MoviePlatformAPI.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)] 
    public string Username { get; set; } = string.Empty;
    
    [Required, EmailAddress] 
    [MaxLength(100)] 
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)] 
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(256)] 
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public List<Movie> Movies { get; set; } = new List<Movie>();
}