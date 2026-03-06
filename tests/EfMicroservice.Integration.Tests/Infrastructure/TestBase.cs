using EfMicroservice.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EfMicroservice.Integration.Tests.Infrastructure;

public abstract class TestBase : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected TestBase(IntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Reset DB state before each test
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await ResetDatabaseAsync(dbContext);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected virtual Task ResetDatabaseAsync(ApplicationDbContext dbContext) => Task.CompletedTask;
}
