using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfMicroservice.Common.Persistence.Resolvers.Interfaces
{
    public interface IChangeTrackingResolver
    {
        void Resolve(EntityEntry entry);
    }
}
