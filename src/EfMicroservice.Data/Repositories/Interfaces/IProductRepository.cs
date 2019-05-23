using System;
using EfMicroservice.Core.Data.Repositories.Interfaces;
using EfMicroservice.Data.Models;

namespace EfMicroservice.Data.Repositories.Interfaces
{
    public interface IProductRepository : IBaseRepository<ProductEntity, Guid>
    {
    }
}
