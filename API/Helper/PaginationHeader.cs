namespace API.Helper;

public class PaginationHeader(int pageNumber, int pageSize, int totalCount, int totalPages)
{
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
    public int TotalCount { get; set; } = totalCount;
    public int TotalPages { get; set; } = totalPages;
}
