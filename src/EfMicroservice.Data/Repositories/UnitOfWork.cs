using System.Threading.Tasks;
using EfMicroservice.Core.Data;
using EfMicroservice.Core.Data.Extensions;
using EfMicroservice.Data.Contexts;
using EfMicroservice.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerFactory _loggerFactory;

        private IProductRepository _productRepository;

        public UnitOfWork(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _loggerFactory = loggerFactory;
        }

        public IProductRepository Products
        {
            get
            {
                return _productRepository = _productRepository ?? new ProductRepository(_dbContext, _loggerFactory);
            }
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
