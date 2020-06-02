﻿using EfMicroservice.Domain.Orders.Exceptions;
using System;
using System.Linq;

namespace EfMicroservice.Domain.Orders
{
    public enum OrderStatuses : int
    {
        Submitted = 1,
        Paid = 2,
        Shipped = 3,
        Cancelled = 4
    }

    public class OrderStatus
    {
        public OrderStatuses Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
