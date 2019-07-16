using Microsoft.EntityFrameworkCore;

namespace EfMicroservice.Common.Persistence.Extensions
{
    public static class DbContextExtensions
    {
        public static void UpdateRowVersion(this DbContext context, IVersionInfo versionInfo, byte[] newRowVersion)
        {
            context.Entry(versionInfo).Property(u => u.RowVersion).OriginalValue = newRowVersion;
        }
    }
}