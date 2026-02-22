using Blog.Application.Abstractions.Realtime;
using Blog.Application.Realtime;
using Blog.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Blog.Api.Realtime;

public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<SignalRNotificationPublisher> _logger;

    public SignalRNotificationPublisher(
        IHubContext<NotificationsHub> hubContext,
        ILogger<SignalRNotificationPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishPostCreatedAsync(PostCreatedNotification notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.SendAsync("PostCreated", new
        {
            id = notification.PostId,
            title = notification.Title,
            authorName = notification.AuthorName,
            createdAt = notification.CreatedAt
        }, cancellationToken);

        _logger.LogInformation("SignalR PostCreated event dispatched for post {PostId}", notification.PostId);
    }
}
