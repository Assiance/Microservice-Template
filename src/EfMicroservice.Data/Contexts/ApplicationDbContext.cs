using System;
using System.Collections.Generic;
using System.Text;
using EfMicroservice.Data.EntityConfigurations;
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
            modelBuilder.ApplyConfiguration(new ValueConfiguration());
        }
    }
}
