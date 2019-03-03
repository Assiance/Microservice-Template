using System;
using System.Linq;
using System.Threading.Tasks;
using EfMicroservice.Core.Data.Repositories.Interfaces;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EfMicroservice.Core.Data.Repositories
{
    public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
        where TEntity : BaseEntity<TKey>
        where TKey : struct 
    {
        private readonly DbContext _dbContext;

        protected BaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<TEntity> Queryable => _dbContext.Set<TEntity>().AsNoTracking();

        public virtual async Task<TEntity> FindAsync(TKey id)
        {
            return await _dbContext.Set<TEntity>().FindAsync(id);
        }

        public virtual TEntity Find(TKey id)
        {
            return _dbContext.Set<TEntity>().Find(id);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            var entityEntry = await _dbContext.Set<TEntity>().AddAsync(entity);
            return entityEntry.Entity;
        }

        public virtual TEntity Add(TEntity entity)
        {
            var entityEntry = _dbContext.Set<TEntity>().Add(entity);
            return entityEntry.Entity;
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public virtual void Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual async Task RemoveAsync(TKey id)
        {
            var retrievedProduct = await FindAsync(id);
            if (retrievedProduct == null)
            {
                throw new NotFoundException();
            }

            _dbContext.Set<TEntity>().Remove(retrievedProduct);
        }

        public virtual void Remove(TKey id)
        {
            var retrievedProduct = Find(id);
            if (retrievedProduct == null)
            {
                throw new NotFoundException();
            }

            _dbContext.Set<TEntity>().Remove(retrievedProduct);
        }
    }
}
