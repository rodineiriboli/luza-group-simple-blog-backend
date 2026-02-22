using Blog.Application.Common.Pagination;
using Blog.Application.Posts.Commands;
using Blog.Application.Posts.Dtos;

namespace Blog.Application.Services.Interfaces;

public interface IPostService
{
    Task<PagedResult<PostSummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PostDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PostDetailsDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken);
    Task<PostDetailsDto> UpdateAsync(Guid postId, Guid userId, UpdatePostRequest request, string? ifMatchHeader, CancellationToken cancellationToken);
    Task DeleteAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
}
