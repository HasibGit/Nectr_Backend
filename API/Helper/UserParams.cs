namespace API.Helper;

public class UserParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value >= MaxPageSize ? MaxPageSize : value;
    }
    public string? Gender { get; set; }
    public string? UserName { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}
