using SecureAppServiceApiLite.Contracts.Messaging;

namespace SecureAppServiceApiLite.Api.Messaging;

public interface IMessageQueue
{
    Task EnqueueAsync(QueuedMessage message, CancellationToken cancellationToken);
}