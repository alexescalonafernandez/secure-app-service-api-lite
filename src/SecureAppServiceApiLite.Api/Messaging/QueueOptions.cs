namespace SecureAppServiceApiLite.Api.Messaging;

public sealed class QueueOptions
{
    public const string SectionName = "QueueOptions";

    public string Provider { get; init; } = "InMemory";

    public string? StorageAccountName { get; init; }

    public string QueueName { get; init; } = "incoming-messages";
}