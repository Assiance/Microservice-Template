using System.Reflection;
using EfMicroservice.Common.Persistence.Extensions;
using EfMicroservice.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EfMicroservice.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {          
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
