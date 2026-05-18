namespace SecureAppServiceApiLite.Api.Contracts;

public sealed record CreateMessageResponse(
    Guid MessageId,
    string Status);
