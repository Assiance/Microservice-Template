﻿namespace EfMicroservice.Api.Models.Products
{
    public abstract class BaseProductRequest
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
    }
}