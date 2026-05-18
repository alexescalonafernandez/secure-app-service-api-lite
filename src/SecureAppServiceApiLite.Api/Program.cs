var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();

public partial class Program;