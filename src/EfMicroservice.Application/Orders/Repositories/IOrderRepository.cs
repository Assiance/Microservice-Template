using EfMicroservice.Common.Persistence.Repositories.Interfaces;
using EfMicroservice.Domain.Orders;

namespace EfMicroservice.Application.Orders.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order, int>
    {
    }
}