using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Repositories;
using EfMicroservice.Application.Products.Repositories;
using EfMicroservice.Common.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfMicroservice.Application.Shared.Repositories
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }

        IOrderRepository Orders { get; }

        Task SaveAsync();

        void Save();

        Task<IDbContextTransaction> BeginTransactionAsync();

        IDbContextTransaction BeginTransaction();

        void UpdateRowVersion(IVersionInfo versionInfo, byte[] newRowVersion);
    }
}