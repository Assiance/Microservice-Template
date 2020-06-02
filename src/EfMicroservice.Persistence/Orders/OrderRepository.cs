using EfMicroservice.Application.Orders.Repositories;
using EfMicroservice.Domain.Orders;
using EfMicroservice.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IList<Order>> FindUnShippedOrdersByProductIdAsync(Guid productId)
        {
            return await _dbContext.Orders.Where(x => x.ProductId == productId && x.StatusId != OrderStatus.Shipped).ToListAsync();
        }
    }
}