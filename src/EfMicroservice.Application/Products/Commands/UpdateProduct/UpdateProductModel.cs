namespace EfMicroservice.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductModel
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
