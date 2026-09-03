using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NexoFleet.Api.IntegrationTests;

public sealed class CoreEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CoreEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task SecuredEndpointsShouldReturnUnauthorizedWhenAnonymous(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerShouldExposeCoreEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paths = document.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/companies", out _));
        Assert.True(paths.TryGetProperty("/api/v1/clients", out _));
        Assert.True(paths.TryGetProperty("/api/v1/employees", out _));
        Assert.True(paths.TryGetProperty("/api/v1/vehicles", out _));
    }
}
