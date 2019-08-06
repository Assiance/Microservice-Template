﻿using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.UnitTests.Builders;
using FluentValidation;
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
            var placeOrder = new PlaceOrderModel
            {
                Quantity = quantity
            };

            var order = new OrderBuilder()
                .WithQuantity(quantity)
                .Build();

            _orderMapperMock.Setup(x => x.Map(placeOrder)).Returns(order);

            //Act
            var sut = CreateSut();
            var result = await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(placeOrder));
            
            //Assert
            Assert.Contains( "'Quantity' must be greater than '0'", result.Message);
        }
    }
}