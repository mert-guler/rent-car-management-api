using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace RentCarManagement.Api.Tests;

public sealed class ApiSmokeTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task VehiclesEndpoint_IsPublicAndReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/vehicles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RentalHistory_RequiresAuthentication()
    {
        var response = await _client.GetAsync("/api/rentals");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Frontend_IsServedByTheApiHost()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("yol.", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/assets/index-", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SwaggerUi_IsAvailableInDevelopmentTestHost()
    {
        var response = await _client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-test-signing-key-at-least-32-bytes");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "RentCarTests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "RentCarTests");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseWebRoot(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "integration-test-signing-key-at-least-32-bytes",
            ["Jwt:Issuer"] = "RentCarTests",
            ["Jwt:Audience"] = "RentCarTests"
        }));
    }
}
