namespace API.Helper;

public class MessagesParams : PaginationParams
{
    public string Username { get; set; } = string.Empty;
    public string Container { get; set; } = "Unread";
}
