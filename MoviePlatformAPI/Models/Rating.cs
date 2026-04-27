namespace MoviePlatformAPI.Models;

public class Rating
{
    public int Id { get; set; }
    
    
    public int Score { get; set; }
    
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
}