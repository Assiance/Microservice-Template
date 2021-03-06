﻿using EfMicroservice.Domain.Orders;
using EfMicroservice.Domain.Products;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Omni.BuildingBlocks.Persistence.Extensions;

namespace EfMicroservice.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductStatus> ProductStatuses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

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