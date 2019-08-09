using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Persistence.Repositories.Interfaces
{
    public interface IBaseRepository<TEntity, in TKey>
        where TEntity : BaseEntity<TKey>
        where TKey : struct
    {
        Task<IList<TEntity>> GetAsync();

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