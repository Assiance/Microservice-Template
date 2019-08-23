using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Persistence.Resolvers.Interfaces
{
    public interface IChangeTrackingResolverAsync
    {
        Task ResolveAsync(EntityEntry entry);
    }
}
