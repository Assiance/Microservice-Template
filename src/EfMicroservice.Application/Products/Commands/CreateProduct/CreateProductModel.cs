using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductModel
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
