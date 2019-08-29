using System.ComponentModel.DataAnnotations;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}