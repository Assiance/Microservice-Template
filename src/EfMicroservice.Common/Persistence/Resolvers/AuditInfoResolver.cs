using EfMicroservice.Common.Persistence.Resolvers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace EfMicroservice.Common.Persistence.Resolvers
{
    public class AuditInfoResolver : IAuditInfoResolver
    {
        public void Resolve(EntityEntry entry)
        {
            if (entry.Entity is IAuditInfo audit)
            {
                var now = DateTime.UtcNow;
                var user = "SYSTEM";

                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.CreatedDate = now;
                        audit.CreatedBy = user;
                        break;

                    case EntityState.Modified:
                        audit.ModifiedDate = now;
                        audit.ModifiedBy = user;
                        break;
                }
            }
        }
    }
}
