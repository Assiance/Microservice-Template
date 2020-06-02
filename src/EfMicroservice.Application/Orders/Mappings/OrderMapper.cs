using System;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Queries;
using EfMicroservice.Domain.Orders;

namespace EfMicroservice.Application.Orders.Mappings
{
    public class OrderMapper : IOrderMapper
    {
        public OrderModel Map(Order source)
        {
            return new OrderModel()
            {
                Id = source.Id,
                Status = source.StatusId.ToString(),
                ProductId = source.ProductId,
                Quantity = source.Quantity
            };
        }

        public Order Map(PlaceOrderCommand source)
        {
            return new Order(source.ProductId, source.Quantity);
        }
    }
}