using System;
using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Mappings;
using EfMicroservice.Application.Orders.Queries;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Orders;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public class PlaceOrderCommand : IPlaceOrderCommand
    {
        private readonly IOrderMapper _orderMapper;
        private readonly IUnitOfWork _unitOfWork;

        public PlaceOrderCommand(IOrderMapper orderMapper, IUnitOfWork unitOfWork)
        {
            _orderMapper = orderMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderModel> ExecuteAsync(PlaceOrderModel orderToCreate)
        {
            var order = _orderMapper.Map(orderToCreate);
            var validator = new OrderValidator();

            validator.ValidateAndThrow(order);
            var createdOrder = await _unitOfWork.Orders.AddAsync(order);

            await _unitOfWork.SaveAsync();

            return _orderMapper.Map(createdOrder);
        }
    }
}