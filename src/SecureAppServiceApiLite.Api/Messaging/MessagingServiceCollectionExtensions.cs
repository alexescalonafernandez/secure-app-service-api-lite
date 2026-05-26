namespace SecureAppServiceApiLite.Api.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessageQueue(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<QueueOptions>(
            configuration.GetSection(QueueOptions.SectionName));

        var queueProvider = configuration
            .GetSection(QueueOptions.SectionName)
            .GetValue<string>("Provider") ?? "InMemory";

        if (string.Equals(queueProvider, "AzureStorage", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IMessageQueue, AzureStorageMessageQueue>();
        }
        else
        {
            services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
        }

        return services;
    }
}
