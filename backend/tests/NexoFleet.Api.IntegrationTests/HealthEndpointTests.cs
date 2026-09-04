using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NexoFleet.Api.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetHealthShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserShouldRequireAuthentication()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCsrfShouldReturnARequestToken()
    {
        var response = await _client.GetAsync("/api/v1/auth/csrf");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task LoginWithoutCsrfShouldBeRejected()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { Email = "user@nexofleet.test", Password = "password" });

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "Security.InvalidAntiforgeryToken",
            problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SwaggerShouldExposeTheApiDocument()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("NexoFleet API", document.GetProperty("info").GetProperty("title").GetString());
        Assert.True(document.GetProperty("paths").TryGetProperty("/api/v1/auth/login", out _));
    }
}
