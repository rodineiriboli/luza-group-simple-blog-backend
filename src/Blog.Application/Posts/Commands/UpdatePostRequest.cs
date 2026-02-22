namespace Blog.Application.Posts.Commands;

public sealed class UpdatePostRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? RowVersion { get; init; }
}
