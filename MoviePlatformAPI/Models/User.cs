using System.ComponentModel.DataAnnotations;

namespace MoviePlatformAPI.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required ,EmailAddress] 
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PasswordHash  { get; set; } = string.Empty;

    public List<Movie> Movies { get; set; } = new List<Movie>();



}