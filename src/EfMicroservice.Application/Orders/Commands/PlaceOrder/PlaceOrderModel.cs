using System;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public class PlaceOrderModel
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
