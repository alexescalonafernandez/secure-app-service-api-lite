using SecureAppServiceApiLite.Contracts.Messaging;
using System.Text.Json;

namespace SecureAppServiceApiLite.QueueConsumer.Processing;

public sealed class QueueMessageProcessor
{
    private static readonly string[] AllowedPriorities = ["Low", "Normal", "High"];

    public ProcessedQueueMessage Process(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw InvalidEnvelope();
        }

        QueuedMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<QueuedMessage>(payload);
        }
        catch (JsonException exception)
        {
            throw new QueueMessageProcessingException(
                QueueMessageFailureReason.InvalidJson,
                "Queue message payload is not valid JSON.",
                exception);
        }

        if (message is null)
        {
            throw InvalidEnvelope();
        }

        if (message.Id == Guid.Empty ||
            message.CreatedAtUtc == default ||
            string.IsNullOrWhiteSpace(message.Subject) ||
            message.Subject.Length > 120 ||
            string.IsNullOrWhiteSpace(message.Body) ||
            message.Body.Length > 2000)
        {
            throw InvalidEnvelope();
        }

        var normalizedPriority = AllowedPriorities.SingleOrDefault(priority =>
            string.Equals(priority, message.Priority?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalizedPriority is null)
        {
            throw InvalidEnvelope();
        }

        return new ProcessedQueueMessage(message.Id, normalizedPriority);
    }

    private static QueueMessageProcessingException InvalidEnvelope() =>
        new(
            QueueMessageFailureReason.InvalidEnvelope,
            "Queue message payload failed contract validation.");
}

public sealed record ProcessedQueueMessage(
    Guid MessageId,
    string Priority);

public enum QueueMessageFailureReason
{
    InvalidJson,
    InvalidEnvelope
}

public sealed class QueueMessageProcessingException : Exception
{
    public QueueMessageProcessingException(
        QueueMessageFailureReason failureReason,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        FailureReason = failureReason;
    }

    public QueueMessageFailureReason FailureReason { get; }
}