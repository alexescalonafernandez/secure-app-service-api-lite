using SecureAppServiceApiLite.Contracts.Messaging;
using System.Collections.Concurrent;

namespace SecureAppServiceApiLite.Api.Messaging;

public sealed class InMemoryMessageQueue : IMessageQueue
{
    private readonly ConcurrentQueue<QueuedMessage> _messages = new();

    public Task EnqueueAsync(QueuedMessage message, CancellationToken cancellationToken)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}