using System;
using EfMicroservice.Common.Persistence.Repositories.Interfaces;
using EfMicroservice.Domain.Products;

namespace EfMicroservice.Application.Products.Repositories
{
    public interface IProductRepository : IBaseRepository<Product, Guid>
    {
    }
}