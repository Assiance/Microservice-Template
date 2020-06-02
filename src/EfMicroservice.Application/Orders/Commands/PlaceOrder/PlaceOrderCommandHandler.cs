using EfMicroservice.Application.Orders.Mappings;
using EfMicroservice.Application.Orders.Queries;
using EfMicroservice.Application.Shared.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
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
}