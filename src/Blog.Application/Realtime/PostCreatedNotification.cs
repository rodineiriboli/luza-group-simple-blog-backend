namespace Blog.Application.Realtime;

public sealed class PostCreatedNotification
{
    public Guid PostId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
