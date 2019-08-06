using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace EfMicroservice.Common.Persistence.Repositories.Interfaces
{
    public interface IBaseRepository<TEntity, in TKey>
        where TEntity : BaseEntity<TKey>
        where TKey : struct
    {
        IQueryable<TEntity> Queryable { get; }

        IIncludableQueryable<TEntity, TProperty> Include<TProperty>(
            Expression<Func<TEntity, TProperty>> navigationPropertyPath) where TProperty : class;

        Task<EntityEntry<TEntity>> AddAsync(TEntity entity);
        EntityEntry<TEntity> Add(TEntity entity);
        Task<TEntity> FindAsync(TKey id);
        TEntity Find(TKey id);
        Task UpdateAsync(TEntity entity);
        void Update(TEntity entity);
        Task RemoveAsync(TKey id);
        void Remove(TKey id);
    }
}