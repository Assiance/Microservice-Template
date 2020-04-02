using AutoFixture;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using MediatR;
using Moq;

namespace EfMicroservice.Application.UnitTests.Orders.Commands.PlaceOrder.PlaceOrderCommandTests
{
    public class PlaceOrderCommandSpec
    {
        protected readonly IFixture _fixture;
        protected readonly Mock<IOrderMapper> _orderMapperMock;
        protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
        protected readonly Mock<IMediator> _mediatorMock;

        public PlaceOrderCommandSpec()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _orderMapperMock = new Mock<IOrderMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mediatorMock = new Mock<IMediator>();
        }

        public PlaceOrderCommandHandler CreateSut()
        {
            return new PlaceOrderCommandHandler(_orderMapperMock.Object, _unitOfWorkMock.Object);
        }
    }
}