using System.Reflection;
using EfMicroservice.Core.Data.Extensions;
using EfMicroservice.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EfMicroservice.Data.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ProductEntity> Products { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {          
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.ApplyVersionInfoConfiguration();
        }
    }
}
