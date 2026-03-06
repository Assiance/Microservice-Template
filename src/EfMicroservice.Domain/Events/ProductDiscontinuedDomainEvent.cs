using EfMicroservice.Domain.Products;
using Omni.BuildingBlocks.Application.Events;

namespace EfMicroservice.Domain.Events
{
    public class ProductDiscontinuedDomainEvent : IDomainEvent
    {
        public Product Product { get; private set; }

        public ProductDiscontinuedDomainEvent(Product product)
        {
            Product = product;
        }
    }
}
