using EfMicroservice.Application.Orders.Repositories;
using EfMicroservice.Domain.Orders;
using EfMicroservice.Persistence.Contexts;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Persistence.Repositories;

namespace EfMicroservice.Persistence.Orders
{
    public class OrderRepository : BaseRepository<Order, int>, IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public OrderRepository(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<OrderRepository>();
        }
    }
}