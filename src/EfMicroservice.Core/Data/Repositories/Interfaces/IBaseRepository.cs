using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Core.Data.Repositories.Interfaces
{
    public interface IBaseRepository<TEntity, in TKey>
        where TEntity : BaseEntity<TKey>
        where TKey : struct
    {
        IQueryable<TEntity> Queryable { get; }

        Task<TEntity> AddAsync(TEntity entity);
        TEntity Add(TEntity entity);
        Task<TEntity> FindAsync(TKey id);
        TEntity Find(TKey id);
        Task UpdateAsync(TEntity entity);
        void Update(TEntity entity);
        Task RemoveAsync(TKey id);
        void Remove(TKey id);
    }
}