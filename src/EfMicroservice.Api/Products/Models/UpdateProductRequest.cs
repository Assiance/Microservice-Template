namespace EfMicroservice.Api.Products.Models
{
    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public byte[] RowVersion { get; set; }
    }
}