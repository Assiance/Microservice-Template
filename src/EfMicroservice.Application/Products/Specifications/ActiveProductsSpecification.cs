using EfMicroservice.Domain.Products;
using Omni.BuildingBlocks.Application.Specification;

namespace EfMicroservice.Application.Products.Specifications
{
    public class ActiveProductsSpecification : Specification<Product>
    {
        public ActiveProductsSpecification()
        {
            AddCriteria(p => p.StatusId != ProductStatuses.Discontinued);
            AddOrderBy(p => p.Name);
        }
    }
}
