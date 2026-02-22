using Blog.Application.Common.Pagination;
using Blog.Application.Posts.Dtos;
using Blog.Domain.Entities;

namespace Blog.Application.Abstractions.Persistence;

public interface IPostRepository
{
    Task<PagedResult<PostSummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PostDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Post?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Post post, CancellationToken cancellationToken);
    void Remove(Post post);
}
