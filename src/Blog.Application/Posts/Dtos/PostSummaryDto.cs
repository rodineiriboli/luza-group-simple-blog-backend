namespace Blog.Application.Posts.Dtos;

public sealed class PostSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
