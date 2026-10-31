using EFCore_LINQ_TASK.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore_LINQ_TASK.DB;


internal class OnlineStoreContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(@"Server=localhost;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(p =>
        {
            p.Property(pr => pr.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.Property(o => o.UnitPrice).HasColumnType("decimal(18,2)");
            oi.Property(o => o.Discount).HasColumnType("decimal(18,2)");
        });
    }
}