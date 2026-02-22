using System.Security.Cryptography;

namespace Blog.Domain.Entities;

public sealed class Post
{
    private Post()
    {
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public static Post Create(string title, string content, Guid authorId)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Content = content.Trim(),
            AuthorId = authorId,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = GenerateRowVersion()
        };
    }

    public void Update(string title, string content)
    {
        Title = title.Trim();
        Content = content.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        RowVersion = GenerateRowVersion();
    }

    private static byte[] GenerateRowVersion()
    {
        return RandomNumberGenerator.GetBytes(8);
    }
}
