using System.Threading.Tasks;
using EfMicroservice.Core.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfMicroservice.Data.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }

        Task SaveAsync();

        void Save();

        Task<IDbContextTransaction> BeginTransactionAsync();

        IDbContextTransaction BeginTransaction();

        void UpdateRowVersion(IVersionInfo versionInfo, byte[] newRowVersion);
    }
}