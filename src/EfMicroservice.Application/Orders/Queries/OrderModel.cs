using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Application.Orders.Queries
{
    public class OrderModel
    {
        public int Id { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
