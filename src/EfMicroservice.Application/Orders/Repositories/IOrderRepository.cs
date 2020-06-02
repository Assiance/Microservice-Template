using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Domain.Orders;
using EfMicroservice.Domain.Products;
using Omni.BuildingBlocks.Persistence.Repositories.Interfaces;

namespace EfMicroservice.Application.Orders.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order, int>
    {
        Task<IList<Order>> FindUnShippedOrdersByProductIdAsync(Guid productId);
    }
}