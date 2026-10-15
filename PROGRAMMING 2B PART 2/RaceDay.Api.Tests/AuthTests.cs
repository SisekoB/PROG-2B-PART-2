using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

public class TestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(x => x.ServiceType == typeof(DbContextOptions<RaceDayContext>));
            services.Remove(descriptor);
            services.AddDbContext<RaceDayContext>(options => options.UseInMemoryDatabase("RaceDayTests"));
        });
    }
}

public class AuthTests : IClassFixture<TestFactory>
{
    private readonly TestFactory _factory;
    public AuthTests(TestFactory factory) => _factory = factory;

    [Fact]
    public async Task RegisterAndLoginWorks()
    {
        var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var email = $"register-{Guid.NewGuid()}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test User", email, "Password1!", "0800000000", "Participant"));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password1!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task ProfileNeedsLogin()
    {
        var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongPasswordIsRejected()
    {
        var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var email = $"wrong-{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test User", email, "Password1!", null, "Participant"));
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "wrong-password"));
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task OrganiserEndpointCannotBeUsedWithoutLogin()
    {
        var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await _client.PostAsJsonAsync("/api/events", new EventRequest("Test Event", "Test", "Running", DateTime.Today.AddDays(10), "Durban", 5, 20));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
