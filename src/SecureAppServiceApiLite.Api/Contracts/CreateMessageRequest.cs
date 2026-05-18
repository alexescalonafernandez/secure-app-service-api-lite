namespace SecureAppServiceApiLite.Api.Contracts;

public sealed record CreateMessageRequest(
    string Subject,
    string Body,
    string? Priority);