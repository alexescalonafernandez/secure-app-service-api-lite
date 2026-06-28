using FluentAssertions;
using SecureAppServiceApiLite.Contracts.Messaging;
using SecureAppServiceApiLite.QueueConsumer.Processing;
using System.Text.Json;

namespace SecureAppServiceApiLite.QueueConsumer.Tests.Processing;

public sealed class QueueMessageProcessorTests
{
    private readonly QueueMessageProcessor _processor = new();

    [Fact]
    public void Process_returns_safe_operational_metadata_for_a_valid_message()
    {
        var message = new QueuedMessage(
            Id: Guid.Parse("9a19a892-1f30-4d33-86ce-5c7f8bd3ec43"),
            Subject: "Monthly invoice",
            Body: "Sensitive invoice details",
            Priority: "high",
            CreatedAtUtc: DateTimeOffset.Parse("2026-06-27T10:00:00+00:00"));

        var result = _processor.Process(JsonSerializer.Serialize(message));

        result.MessageId.Should().Be(message.Id);
        result.Priority.Should().Be("High");
    }

    [Fact]
    public void Process_rejects_invalid_json_without_exposing_the_payload()
    {
        const string payload = "{ invalid-json-and-sensitive-text }";

        Action action = () => _processor.Process(payload);

        var exception = action.Should()
            .Throw<QueueMessageProcessingException>()
            .Which;

        exception.FailureReason.Should().Be(QueueMessageFailureReason.InvalidJson);
        exception.Message.Should().NotContain(payload);
    }

    [Fact]
    public void Process_rejects_an_invalid_envelope_without_exposing_body_content()
    {
        const string sensitiveBody = "do-not-log-this-message-body";

        var message = new QueuedMessage(
            Id: Guid.NewGuid(),
            Subject: string.Empty,
            Body: sensitiveBody,
            Priority: "Normal",
            CreatedAtUtc: DateTimeOffset.UtcNow);

        Action action = () => _processor.Process(JsonSerializer.Serialize(message));

        var exception = action.Should()
            .Throw<QueueMessageProcessingException>()
            .Which;

        exception.FailureReason.Should().Be(QueueMessageFailureReason.InvalidEnvelope);
        exception.Message.Should().NotContain(sensitiveBody);
    }
}
