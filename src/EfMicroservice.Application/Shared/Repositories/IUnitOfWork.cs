using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Repositories;
using EfMicroservice.Application.Products.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Omni.BuildingBlocks.Persistence;

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