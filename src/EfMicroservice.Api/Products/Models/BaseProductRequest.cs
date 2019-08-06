﻿namespace EfMicroservice.Api.Products.Models
{
    public abstract class BaseProductRequest
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}