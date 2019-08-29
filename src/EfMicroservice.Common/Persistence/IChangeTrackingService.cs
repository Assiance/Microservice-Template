using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfMicroservice.Common.Persistence
{
    public interface IChangeTrackingService
    {
        Task ExecuteResolversAsync(EntityEntry entry);
    }
}
