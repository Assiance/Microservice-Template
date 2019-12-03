using EfMicroservice.Domain.Orders;
using Omni.BuildingBlocks.Persistence.Repositories.Interfaces;

namespace EfMicroservice.Application.Orders.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order, int>
    {
    }
}