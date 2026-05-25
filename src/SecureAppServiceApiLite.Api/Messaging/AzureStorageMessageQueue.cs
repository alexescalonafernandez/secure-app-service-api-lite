using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SecureAppServiceApiLite.Api.Messaging;

public sealed class AzureStorageMessageQueue : IMessageQueue
{
    private readonly QueueClient _queueClient;

    public AzureStorageMessageQueue(IOptions<QueueOptions> options)
    {
        var queueOptions = options.Value;

        if (string.IsNullOrWhiteSpace(queueOptions.StorageAccountName))
        {
            throw new InvalidOperationException("QueueOptions:StorageAccountName is required when using AzureStorage queue provider.");
        }

        if (string.IsNullOrWhiteSpace(queueOptions.QueueName))
        {
            throw new InvalidOperationException("QueueOptions:QueueName is required when using AzureStorage queue provider.");
        }

        var queueUri = new Uri($"https://{queueOptions.StorageAccountName}.queue.core.windows.net/{queueOptions.QueueName}");

        _queueClient = new QueueClient(queueUri, new DefaultAzureCredential());
    }

    public async Task EnqueueAsync(QueuedMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);

        await _queueClient.SendMessageAsync(
            messageText: payload,
            cancellationToken: cancellationToken);
    }
}