using AutoFixture;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Domain.Orders;
using System.Threading.Tasks;
using Xunit;

namespace EfMicroservice.Application.UnitTests.Orders.Commands.PlaceOrder.PlaceOrderCommandTests
{
    public class ExecuteAsyncTests : PlaceOrderCommandSpec
    {
        [Fact]
        public async Task ExecuteAsync_Should_Throw_Exception_When_Quantity_Less_Than_Zero()
        {
            //Arrange
            var quantity = -5;
            var placeOrder = new PlaceOrderCommand
            {
                Quantity = quantity
            };

            var order = _fixture.Build<Order>()
                .With(x => x.Quantity, quantity)
                .Create();

            _orderMapperMock.Setup(x => x.Map(placeOrder)).Returns(order);

            //Act
            var sut = CreateSut();
            var result = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => sut.Handle(placeOrder, default));

            //Assert
            Assert.Contains($"'{nameof(Order.Quantity)}' must be greater than '0'", result.Message);
        }
    }
}