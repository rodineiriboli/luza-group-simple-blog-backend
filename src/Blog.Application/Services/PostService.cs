using Blog.Application.Abstractions.Persistence;
using Blog.Application.Abstractions.Realtime;
using Blog.Application.Common.Concurrency;
using Blog.Application.Common.Exceptions;
using Blog.Application.Common.Pagination;
using Blog.Application.Posts.Commands;
using Blog.Application.Posts.Dtos;
using Blog.Application.Realtime;
using Blog.Application.Services.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Blog.Application.Services;

public sealed class PostService : IPostService
{
    private const int MaxPageSize = 50;

    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IValidator<CreatePostRequest> _createValidator;
    private readonly IValidator<UpdatePostRequest> _updateValidator;
    private readonly ILogger<PostService> _logger;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationPublisher notificationPublisher,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        ILogger<PostService> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationPublisher = notificationPublisher;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public Task<PagedResult<PostSummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, MaxPageSize);

        return _postRepository.GetPagedAsync(normalizedPage, normalizedPageSize, cancellationToken);
    }

    public async Task<PostDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        return post ?? throw new NotFoundException("Post not found.");
    }

    public async Task<PostDetailsDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var author = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (author is null)
        {
            throw new UnauthorizedException("User not found for this token.");
        }

        var post = Post.Create(request.Title, request.Content, userId);
        await _postRepository.AddAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdPost = await _postRepository.GetByIdAsync(post.Id, cancellationToken)
            ?? throw new NotFoundException("Post was created but could not be loaded.");

        await _notificationPublisher.PublishPostCreatedAsync(
            new PostCreatedNotification
            {
                PostId = createdPost.Id,
                Title = createdPost.Title,
                AuthorName = createdPost.AuthorName,
                CreatedAt = createdPost.CreatedAt
            },
            cancellationToken);

        _logger.LogInformation("Post created successfully: {PostId} by {UserId}", createdPost.Id, userId);

        return createdPost;
    }

    public async Task<PostDetailsDto> UpdateAsync(
        Guid postId,
        Guid userId,
        UpdatePostRequest request,
        string? ifMatchHeader,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var post = await _postRepository.GetTrackedByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        EnsureOwnership(post, userId);

        var versionInput = string.IsNullOrWhiteSpace(ifMatchHeader) ? request.RowVersion : ifMatchHeader;
        if (!RowVersionCodec.TryDecode(versionInput, out var expectedRowVersion))
        {
            throw new BadRequestException("A valid RowVersion or If-Match header is required.");
        }

        if (!post.RowVersion.SequenceEqual(expectedRowVersion))
        {
            throw new ConflictException("Concurrency conflict detected. Reload and try again.");
        }

        post.Update(request.Title, request.Content);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            throw new ConflictException("Concurrency conflict detected. Reload and try again.");
        }

        _logger.LogInformation("Post updated successfully: {PostId} by {UserId}", postId, userId);

        return await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException("Post not found after update.");
    }

    public async Task DeleteAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetTrackedByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        EnsureOwnership(post, userId);

        _postRepository.Remove(post);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            throw new ConflictException("Concurrency conflict detected while deleting the post.");
        }

        _logger.LogInformation("Post deleted successfully: {PostId} by {UserId}", postId, userId);
    }

    private static void EnsureOwnership(Post post, Guid userId)
    {
        if (post.AuthorId != userId)
        {
            throw new ForbiddenException("You can only modify your own posts.");
        }
    }
}
