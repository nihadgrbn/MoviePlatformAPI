namespace MoviePlatformAPI.DTOs.Shared;

public class PagedResponseDto<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public PaginationMetaDto Meta { get; set; } = new PaginationMetaDto();
    public PaginationLinksDto Links { get; set; } = new PaginationLinksDto();
}
    
