using Blog.Application.Abstractions.Persistence;
using Blog.Application.Common.Concurrency;
using Blog.Application.Common.Pagination;
using Blog.Application.Posts.Dtos;
using Blog.Domain.Entities;
using Blog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public sealed class PostRepository : IPostRepository
{
    private readonly BlogDbContext _dbContext;

    public PostRepository(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PostSummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var baseQuery = _dbContext.Posts
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await _dbContext.Posts
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PostSummaryDto
            {
                Id = x.Id,
                Title = x.Title,
                AuthorId = x.AuthorId,
                AuthorName = x.Author.DisplayName,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PostSummaryDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public Task<PostDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PostDetailsDto
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                AuthorId = x.AuthorId,
                AuthorName = x.Author.DisplayName,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                RowVersion = RowVersionCodec.ToBase64(x.RowVersion)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Post?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Posts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(Post post, CancellationToken cancellationToken)
    {
        return _dbContext.Posts.AddAsync(post, cancellationToken).AsTask();
    }

    public void Remove(Post post)
    {
        _dbContext.Posts.Remove(post);
    }
}
