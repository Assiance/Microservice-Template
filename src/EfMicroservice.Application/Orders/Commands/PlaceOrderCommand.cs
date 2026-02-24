using EfMicroservice.Application.Orders.Mappings;
using EfMicroservice.Application.Orders.Models;
using EfMicroservice.Application.Shared.Repositories;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public class PlaceOrderCommand : IRequest<OrderModel>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderModel>
    {
        private readonly IOrderMapper _orderMapper;
        private readonly IUnitOfWork _unitOfWork;

        public PlaceOrderCommandHandler(IOrderMapper orderMapper, IUnitOfWork unitOfWork)
        {
            _orderMapper = orderMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderModel> Handle(PlaceOrderCommand orderToCreate, CancellationToken cancellationToken)
        {
            var order = _orderMapper.Map(orderToCreate);

            var createdOrder = await _unitOfWork.Orders.AddAsync(order);

            await _unitOfWork.SaveAsync();

            return _orderMapper.Map(createdOrder);
        }
    }

    public class PlaceOrderModelValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderModelValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}