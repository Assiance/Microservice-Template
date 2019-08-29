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
using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Persistence.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly ILoggerFactory _loggerFactory;

        private IProductRepository _productRepository;
        private IOrderRepository _orderRepository;

        public UnitOfWork(ApplicationDbContext dbContext, IChangeTrackingService changeTrackingService, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _changeTrackingService = changeTrackingService;
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
            await OnBeforeSaveChangesAsync();
            await _dbContext.SaveChangesAsync();
        }

        public void Save()
        {
            OnBeforeSaveChangesAsync().GetAwaiter();
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

        private async Task OnBeforeSaveChangesAsync()
        {
            var entries = _dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                await _changeTrackingService.ExecuteResolversAsync(entry);
            }
        }
    }
}