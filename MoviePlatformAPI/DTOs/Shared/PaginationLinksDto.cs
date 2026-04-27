namespace MoviePlatformAPI.DTOs.Shared;

public class PaginationLinksDto
{
    public string? Self { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public string? First { get; set; }
    public string? Last { get; set; }
}