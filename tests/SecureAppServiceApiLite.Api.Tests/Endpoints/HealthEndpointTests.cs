using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SecureAppServiceApiLite.Api.Tests.Endpoints;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WithExpectedPayload()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Healthy");
        payload.Service.Should().Be("SecureAppServiceApiLite.Api");
    }

    private sealed record HealthResponse(string Status, string Service);
}
