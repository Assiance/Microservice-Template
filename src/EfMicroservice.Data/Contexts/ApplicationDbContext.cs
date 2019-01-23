using System.Linq;
using System.Reflection;
using EfMicroservice.Core.Data;
using EfMicroservice.Core.Data.Extensions;
using EfMicroservice.Data.EntityTypeConfigurations;
using EfMicroservice.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EfMicroservice.Data.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ValueEntity> Values { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {          
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ValueEntityTypeConfiguration());

            modelBuilder.ApplyVersionInfoConfiguration();
        }
    }
}
