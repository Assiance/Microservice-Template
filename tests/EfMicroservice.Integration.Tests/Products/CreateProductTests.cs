using EfMicroservice.Integration.Tests.Infrastructure;
using EfMicroservice.Persistence.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EfMicroservice.Integration.Tests.Products;

public class CreateProductTests : TestBase
{
    public CreateProductTests(IntegrationTestWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201AndPersists()
    {
        // Arrange
        var request = new
        {
            Name = "Test Widget",
            Price = 19.99m,
            Quantity = 100
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = db.Products.FirstOrDefault(p => p.Name == "Test Widget");
        product.Should().NotBeNull();
        product!.Price.Should().Be(19.99m);
    }

    [Fact]
    public async Task CreateProduct_MissingName_Returns400()
    {
        // Arrange
        var request = new { Price = 9.99m, Quantity = 10 };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
