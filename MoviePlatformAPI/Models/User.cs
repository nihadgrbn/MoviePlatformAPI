using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace MoviePlatformAPI.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required, EmailAddress] 
    [JsonIgnore] 
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [JsonIgnore] 
    public string PasswordHash { get; set; } = string.Empty;

    [JsonIgnore] 
    public List<Movie> Movies { get; set; } = new List<Movie>();
}