using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Repositories;
using EfMicroservice.Application.Products.Repositories;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Common.Persistence;
using EfMicroservice.Common.Persistence.Extensions;
using EfMicroservice.Persistence.Contexts;
using EfMicroservice.Persistence.Orders;
using EfMicroservice.Persistence.Products;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Persistence.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerFactory _loggerFactory;

        private IProductRepository _productRepository;
        private IOrderRepository _orderRepository;

        public UnitOfWork(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _loggerFactory = loggerFactory;
        }

        public IProductRepository Products
        {
            get { return _productRepository = _productRepository ?? new ProductRepository(_dbContext, _loggerFactory); }
        }

        public IOrderRepository Orders
        {
            get { return _orderRepository = _orderRepository ?? new OrderRepository(_dbContext, _loggerFactory); }
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }

        public void UpdateRowVersion(IVersionInfo versionInfo, byte[] newRowVersion)
        {
            _dbContext.UpdateRowVersion(versionInfo, newRowVersion);
        }
    }
}