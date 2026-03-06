using EfMicroservice.Integration.Tests.Infrastructure;
using EfMicroservice.Persistence.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EfMicroservice.Integration.Tests.Products;

public class OutboxTests : TestBase
{
    public OutboxTests(IntegrationTestWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DiscontinueProduct_CreatesOutboxMessage()
    {
        // Arrange — first create a product
        var createRequest = new { Name = "Outbox Test Product", Price = 5.00m, Quantity = 50 };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = db.Products.First(p => p.Name == "Outbox Test Product");

        // Act — discontinue the product
        var discontinueResponse = await Client.PutAsync($"/api/v1/products/{product.Id}/discontinue", null);
        discontinueResponse.IsSuccessStatusCode.Should().BeTrue();

        // Assert — outbox message exists for the integration event
        using var scope2 = Factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var outboxMessages = db2.OutboxMessages.ToList();
        outboxMessages.Should().NotBeEmpty("a ProductDiscontinued integration event should be written to the outbox");
        outboxMessages.All(m => m.ProcessedAt == null).Should().BeTrue("messages should be unprocessed initially");
    }
}
