using EfMicroservice.Domain.Products;
using MediatR;

namespace EfMicroservice.Domain.Events
{
    public class ProductDiscontinuedDomainEvent : INotification
    {
        public Product Product { get; private set; }

        public ProductDiscontinuedDomainEvent(Product product)
        {
            Product = product;
        }
    }
}
