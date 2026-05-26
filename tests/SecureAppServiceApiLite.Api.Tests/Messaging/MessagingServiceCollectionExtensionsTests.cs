using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureAppServiceApiLite.Api.Messaging;

namespace SecureAppServiceApiLite.Api.Tests.Messaging;

public sealed class MessagingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMessageQueue_ShouldRegisterInMemoryQueue_WhenProviderIsMissing()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        services.AddMessageQueue(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var messageQueue = serviceProvider.GetRequiredService<IMessageQueue>();

        messageQueue.Should().BeOfType<InMemoryMessageQueue>();
    }

    [Fact]
    public void AddMessageQueue_ShouldRegisterInMemoryQueue_WhenProviderIsInMemory()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["QueueOptions:Provider"] = "InMemory"
        });
        var services = new ServiceCollection();

        services.AddMessageQueue(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var messageQueue = serviceProvider.GetRequiredService<IMessageQueue>();

        messageQueue.Should().BeOfType<InMemoryMessageQueue>();
    }

    [Fact]
    public void AddMessageQueue_ShouldRegisterAzureStorageQueue_WhenProviderIsAzureStorage()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["QueueOptions:Provider"] = "AzureStorage",
            ["QueueOptions:StorageAccountName"] = "teststorageaccount",
            ["QueueOptions:QueueName"] = "incoming-messages"
        });
        var services = new ServiceCollection();

        services.AddMessageQueue(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var messageQueue = serviceProvider.GetRequiredService<IMessageQueue>();

        messageQueue.Should().BeOfType<AzureStorageMessageQueue>();
    }

    [Fact]
    public void AddMessageQueue_ShouldRegisterAzureStorageQueue_WhenProviderIsAzureStorageWithDifferentCasing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["QueueOptions:Provider"] = "azurestorage",
            ["QueueOptions:StorageAccountName"] = "teststorageaccount",
            ["QueueOptions:QueueName"] = "incoming-messages"
        });
        var services = new ServiceCollection();

        services.AddMessageQueue(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var messageQueue = serviceProvider.GetRequiredService<IMessageQueue>();

        messageQueue.Should().BeOfType<AzureStorageMessageQueue>();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? settings = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
