using System;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Queries;
using EfMicroservice.Domain.Orders;

namespace EfMicroservice.Application.Orders.Mappings
{
    public interface IOrderMapper
    {
        OrderModel Map(Order source);
        Order Map(PlaceOrderModel source);
    }
}