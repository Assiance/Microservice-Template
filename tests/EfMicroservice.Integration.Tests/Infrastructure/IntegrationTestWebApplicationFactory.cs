using EfMicroservice.Api;
using EfMicroservice.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace EfMicroservice.Integration.Tests.Infrastructure;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("integration_test_db")
        .WithUsername("postgres")
        .WithPassword("test_password")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext and pool registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                            d.ServiceType == typeof(ApplicationDbContext))
                .ToList();
            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            // Add Testcontainers PostgreSQL
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString())
                       .UseSnakeCaseNamingConvention());
        });

        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
