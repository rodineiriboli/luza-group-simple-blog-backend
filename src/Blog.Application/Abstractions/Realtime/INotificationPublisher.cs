using Blog.Application.Realtime;

namespace Blog.Application.Abstractions.Realtime;

public interface INotificationPublisher
{
    Task PublishPostCreatedAsync(PostCreatedNotification notification, CancellationToken cancellationToken);
}
