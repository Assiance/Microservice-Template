using EfMicroservice.Common.Persistence;
using EfMicroservice.Common.Persistence.Resolvers.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;

namespace EfMicroservice.Persistence.ChangeTracking
{
    public class ChangeTrackingService : IChangeTrackingService
    {
        private readonly IAuditInfoResolver _auditInfoResolver;

        public ChangeTrackingService(IAuditInfoResolver auditInfoResolver)
        {
            _auditInfoResolver = auditInfoResolver;
        }

        public async Task ExecuteResolversAsync(EntityEntry entry)
        {
            _auditInfoResolver.Resolve(entry);
        }
    }
}
