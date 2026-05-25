using FluentValidation;
using SecureAppServiceApiLite.Api.Contracts;
using SecureAppServiceApiLite.Api.Messaging;
using SecureAppServiceApiLite.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<CreateMessageRequestValidator>();

builder.Services.Configure<QueueOptions>(
    builder.Configuration.GetSection(QueueOptions.SectionName));

var queueProvider = builder.Configuration
    .GetSection(QueueOptions.SectionName)
    .GetValue<string>("Provider") ?? "InMemory";

if (string.Equals(queueProvider, "AzureStorage", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IMessageQueue, AzureStorageMessageQueue>();
}
else
{
    builder.Services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "SecureAppServiceApiLite.Api"
}))
.WithName("GetHealth");

app.MapPost("/api/messages", async (
    CreateMessageRequest request,
    IValidator<CreateMessageRequest> validator,
    IMessageQueue messageQueue,
    CancellationToken cancellationToken) =>
{
    var validationResult = await validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var message = new QueuedMessage(
        Id: Guid.NewGuid(),
        Subject: request.Subject,
        Body: request.Body,
        Priority: string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority,
        CreatedAtUtc: DateTimeOffset.UtcNow);

    await messageQueue.EnqueueAsync(message, cancellationToken);

    var response = new CreateMessageResponse(
        MessageId: message.Id,
        Status: "Accepted");

    return Results.Accepted($"/api/messages/{message.Id}", response);
})
.WithName("CreateMessage");

app.Run();

public partial class Program;