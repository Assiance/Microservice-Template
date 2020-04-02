using EfMicroservice.Application.Orders.Queries;
using MediatR;
using System;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public class PlaceOrderCommand : IRequest<OrderModel>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}