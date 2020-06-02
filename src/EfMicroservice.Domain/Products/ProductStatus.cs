namespace EfMicroservice.Domain.Products
{
    public enum ProductStatuses : int
    {
        InStock = 1,
        OutOfStock = 2,
        Discontinued = 3
    }

    public class ProductStatus
    {
        public ProductStatuses Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
