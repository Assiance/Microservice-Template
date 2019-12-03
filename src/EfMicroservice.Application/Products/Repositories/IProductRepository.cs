using System;
using EfMicroservice.Domain.Products;
using Omni.BuildingBlocks.Persistence.Repositories.Interfaces;

namespace EfMicroservice.Application.Products.Repositories
{
    public interface IProductRepository : IBaseRepository<Product, Guid>
    {
    }
}