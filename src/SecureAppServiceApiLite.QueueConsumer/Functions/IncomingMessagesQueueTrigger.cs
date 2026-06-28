using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SecureAppServiceApiLite.QueueConsumer.Processing;

namespace SecureAppServiceApiLite.QueueConsumer.Functions;

public sealed class IncomingMessagesQueueTrigger
{
    private readonly ILogger<IncomingMessagesQueueTrigger> _logger;
    private readonly QueueMessageProcessor _processor;

    public IncomingMessagesQueueTrigger(
        ILogger<IncomingMessagesQueueTrigger> logger,
        QueueMessageProcessor processor)
    {
        _logger = logger;
        _processor = processor;
    }

    [Function(nameof(IncomingMessagesQueueTrigger))]
    public Task Run(
        [QueueTrigger("incoming-messages", Connection = "IncomingMessagesStorage")]
        string payload)
    {
        try
        {
            var processedMessage = _processor.Process(payload);

            _logger.LogInformation(
                "Queue message processed. MessageId: {MessageId}; Priority: {Priority}; Outcome: {Outcome}",
                processedMessage.MessageId,
                processedMessage.Priority,
                "Succeeded");

            return Task.CompletedTask;
        }
        catch (QueueMessageProcessingException exception)
        {
            _logger.LogWarning(
                "Queue message rejected. FailureReason: {FailureReason}; Outcome: {Outcome}",
                exception.FailureReason,
                "Rejected");

            throw;
        }
    }
}