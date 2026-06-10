using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using SecureAppServiceApiLite.Api.Contracts;

namespace SecureAppServiceApiLite.Api.Tests.Endpoints;

public sealed class CreateMessageEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CreateMessageEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostMessages_ShouldReturnAccepted_WhenRequestIsValid()
    {
        var request = new CreateMessageRequest(
            Subject: "Order update",
            Body: "Order #123 has shipped.",
            Priority: "Normal");

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location.Should().NotBeNull();

        var payload = await response.Content.ReadFromJsonAsync<CreateMessageResponse>(cancellationToken: TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Accepted");
        payload.MessageId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostMessages_ShouldReturnBadRequest_WhenSubjectIsEmpty()
    {
        var request = new CreateMessageRequest(
            Subject: string.Empty,
            Body: "Body value",
            Priority: "Low");

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        validationProblem.Should().NotBeNull();
        validationProblem!.Errors.Keys.Should().Contain(nameof(CreateMessageRequest.Subject));
    }

    [Fact]
    public async Task PostMessages_ShouldReturnBadRequest_WhenBodyIsEmpty()
    {
        var request = new CreateMessageRequest(
            Subject: "Subject",
            Body: string.Empty,
            Priority: "Low");

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        validationProblem.Should().NotBeNull();
        validationProblem!.Errors.Keys.Should().Contain(nameof(CreateMessageRequest.Body));
    }

    [Fact]
    public async Task PostMessages_ShouldReturnBadRequest_WhenPriorityIsUnsupported()
    {
        var request = new CreateMessageRequest(
            Subject: "Subject",
            Body: "Body",
            Priority: "Urgent");

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        validationProblem.Should().NotBeNull();
        validationProblem!.Errors.Keys.Should().Contain(nameof(CreateMessageRequest.Priority));
    }
}
