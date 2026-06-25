namespace SecureAppServiceApiLite.Contracts.Messaging;

public sealed record QueuedMessage(
    Guid Id,
    string Subject,
    string Body,
    string Priority,
    DateTimeOffset CreatedAtUtc);