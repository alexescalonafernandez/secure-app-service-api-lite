using SecureAppServiceApiLite.Api.Contracts;
using SecureAppServiceApiLite.Api.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();

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
    IMessageQueue messageQueue,
    CancellationToken cancellationToken) =>
{
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