using System.ComponentModel.DataAnnotations;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] // no overload for decimal
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}