using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NexoFleet.Api.IntegrationTests;

public sealed class CoreEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoreEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Theory]
    [InlineData("/api/v1/companies")]
    [InlineData("/api/v1/clients")]
    [InlineData("/api/v1/employees")]
    [InlineData("/api/v1/vehicles")]
    [InlineData("/api/v1/routes")]
    [InlineData("/api/v1/route-schedules")]
    [InlineData("/api/v1/trips")]
    [InlineData("/api/v1/payment-periods")]
    [InlineData("/api/v1/payment-reports")]
    [InlineData("/api/v1/notifications")]
    [InlineData("/api/v1/audit-logs")]
    [InlineData("/api/v1/files/sample.pdf")]
    public async Task SecuredEndpointsShouldReturnUnauthorizedWhenAnonymous(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerShouldExposeAllApiEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paths = document.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/companies", out _));
        Assert.True(paths.TryGetProperty("/api/v1/clients", out _));
        Assert.True(paths.TryGetProperty("/api/v1/employees", out _));
        Assert.True(paths.TryGetProperty("/api/v1/vehicles", out _));
        Assert.True(paths.TryGetProperty("/api/v1/routes", out _));
        Assert.True(paths.TryGetProperty("/api/v1/route-schedules", out _));
        Assert.True(paths.TryGetProperty("/api/v1/trips", out _));
        Assert.True(paths.TryGetProperty("/api/v1/payment-periods", out _));
        Assert.True(paths.TryGetProperty("/api/v1/payment-reports", out _));
        Assert.True(paths.TryGetProperty("/api/v1/notifications", out _));
        Assert.True(paths.TryGetProperty("/api/v1/audit-logs", out _));
        Assert.True(paths.TryGetProperty("/api/v1/files/upload", out _));
    }
}
