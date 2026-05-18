namespace SecureAppServiceApiLite.Api.Messaging;

public sealed record QueuedMessage(
    Guid Id,
    string Subject,
    string Body,
    string Priority,
    DateTimeOffset CreatedAtUtc);