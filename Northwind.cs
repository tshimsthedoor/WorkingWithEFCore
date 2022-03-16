using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;

namespace Packt.Shared
{

    // this manage the connection to the database
    public class Northwind :DbContext
    {
        // thsese properties map to the table in the database
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Northwind.db");
            optionsBuilder.UseLazyLoadingProxies()
            .UseSqlite($"Filename={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // example of using Fluent API instead of attributes
            // to limit the length of a category name to 15
            modelBuilder.Entity<Category>()
                    .Property(category => category.CategoryName)
                    .IsRequired() //Not null
                    .HasMaxLength(15);

            // globbal filter to remove discontinued products
            modelBuilder.Entity<Product>()
                    .HasQueryFilter(p => !p.Discontinued);        
        }
    }
}