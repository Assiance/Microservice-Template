﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Api.Orders.Models
{
    public class CreateOrderRequest
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
