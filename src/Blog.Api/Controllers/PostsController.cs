using Blog.Application.Common.Concurrency;
using Blog.Application.Common.Pagination;
using Blog.Application.Posts.Commands;
using Blog.Application.Posts.Dtos;
using Blog.Application.Services.Interfaces;
using Blog.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PostSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PostSummaryDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _postService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var post = await _postService.GetByIdAsync(id, cancellationToken);
        Response.Headers.ETag = RowVersionCodec.ToETag(Convert.FromBase64String(post.RowVersion));

        return Ok(post);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PostDetailsDto>> Create([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var post = await _postService.CreateAsync(userId, request, cancellationToken);
        Response.Headers.ETag = RowVersionCodec.ToETag(Convert.FromBase64String(post.RowVersion));

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostDetailsDto>> Update(Guid id, [FromBody] UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var ifMatchHeader = Request.Headers.IfMatch.ToString();
        var post = await _postService.UpdateAsync(id, userId, request, ifMatchHeader, cancellationToken);
        Response.Headers.ETag = RowVersionCodec.ToETag(Convert.FromBase64String(post.RowVersion));

        return Ok(post);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await _postService.DeleteAsync(id, userId, cancellationToken);

        return NoContent();
    }
}
