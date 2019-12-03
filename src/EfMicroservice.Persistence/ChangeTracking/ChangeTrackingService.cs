using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using Omni.BuildingBlocks.Persistence;
using Omni.BuildingBlocks.Persistence.Resolvers.Interfaces;

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
