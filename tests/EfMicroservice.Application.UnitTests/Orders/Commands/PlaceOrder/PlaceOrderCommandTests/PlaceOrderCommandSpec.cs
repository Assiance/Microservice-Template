using System;
using System.Collections.Generic;
using System.Text;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using Moq;

namespace EfMicroservice.Application.UnitTests.Orders.Commands.PlaceOrder.PlaceOrderCommandTests
{
    public class PlaceOrderCommandSpec
    {
        protected readonly Mock<IOrderMapper> _orderMapperMock;
        protected readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public PlaceOrderCommandSpec()
        {
            _orderMapperMock = new Mock<IOrderMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        public PlaceOrderCommand CreateSut()
        {
            return new PlaceOrderCommand(_orderMapperMock.Object, _unitOfWorkMock.Object);
        }
    }
}