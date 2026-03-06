using EfMicroservice.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Omni.BuildingBlocks.Application.Idempotency;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Persistence.Idempotency
{
    public class EfIdempotencyStore : IIdempotencyStore
    {
        private readonly ApplicationDbContext _dbContext;

        public EfIdempotencyStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsAsync(Guid key, CancellationToken cancellationToken = default)
        {
            return await _dbContext.IdempotencyRecords
                .AnyAsync(r => r.Key == key, cancellationToken);
        }

        public async Task SetAsync(Guid key, string requestType, string responseJson, CancellationToken cancellationToken = default)
        {
            _dbContext.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = key,
                RequestType = requestType,
                Response = responseJson,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> GetResponseAsync(Guid key, CancellationToken cancellationToken = default)
        {
            var record = await _dbContext.IdempotencyRecords
                .FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
            return record?.Response;
        }
    }
}
