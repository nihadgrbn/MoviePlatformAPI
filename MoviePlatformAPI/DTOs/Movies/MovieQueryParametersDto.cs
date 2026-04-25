namespace MoviePlatformAPI.DTOs.Movies;

public class MovieQueryParametersDto
{
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 10;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > 50) ? 50 : value;
    }
    public string? Genre { get; set; } 
    public string? SearchTerm { get; set; } 
    
    public string? SortBy { get; set; } 
    public bool IsDescending { get; set; } = false;
}