namespace EfMicroservice.Api.Products.Models
{
    public class UpdateProductRequest : BaseProductRequest
    {
        public byte[] RowVersion { get; set; }
    }
}