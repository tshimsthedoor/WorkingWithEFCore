using System.Collections.Generic;
using static System.Console;
using Packt.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Packt.Shared
{
    class Program
    {
        static void Main(string[] args)
        {

            //QueryingCategories();
            // QueryingProducts();
            //QueryingWithLike();

            // if(AddProduct(6, "Bob's Burgers", 500F))
            // {
            //     WriteLine("Add product successful.");
            // }

            // if(IncreaseProductPrice("Bob",  27F))
            // {
            //     WriteLine("Update product price successful.");
            // }
            int deleted = DeleteProducts("Bob");
            WriteLine($"{deleted} product(s) were deleted.");
            ListProducts();
        }

        static void QueryingCategories()
        {
            using (var db = new Northwind())
            {
                var loggerFactory = db.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(new ConsoleLoggerProvider());
                WriteLine("Categories and How many products they have:");
                // a query to get all categories and their related products
                IQueryable<Category> cats; // = db.Categories; //.Include(c => c.Products);

                db.ChangeTracker.LazyLoadingEnabled = false;

                Write("Enable eager loading? (Y/N): ");
                bool eagerLoading = (ReadKey().Key == ConsoleKey.Y);
                bool explicitLoading = false;
                WriteLine();
                if (eagerLoading)
                {
                    cats = db.Categories.Include(c => c.Products);
                }
                else
                {
                    cats = db.Categories;
                    Write("Enable explicit loading? (Y/N): ");
                    explicitLoading = (ReadKey().Key == ConsoleKey.Y);
                    WriteLine();

                }
                foreach (Category c in cats)
                {
                    if (explicitLoading)
                    {
                        Write($"Explicitly load products for {c.CategoryName}? (Y/N):");
                        ConsoleKey key = ReadKey().Key;
                        WriteLine();
                        
                        {
                            var products = db.Entry(c).Collection(c2 => c2.Products);
                            if (!products.IsLoaded) products.Load();
                        }

                    }
                    WriteLine($"{c.CategoryName} has {c.Products.Count} products.");
                }

            }
        }

        static void QueryingProducts()
        {
            using (var db = new Northwind())
            {
                var loggerFactory = db.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(new ConsoleLoggerProvider());
                WriteLine("Product that cost more than a price, highest at top.");
                string input;
                float price;

                do
                {
                    Write("Enter a product price: ");
                    input = ReadLine();
                }
                while (!float.TryParse(input, out price));
                IQueryable<Product> prods = db.Products
                                    .TagWith("Products filtered by price and sorted.")
                                    .Where(product => product.Cost > price)
                                    .OrderByDescending(product => product.Cost);

                foreach (Product item in prods)
                {
                    WriteLine("{0}: {1} costs {2:$#,##0.00} and has {3} in stock.",
                     item.ProductID, item.ProductName, item.Cost, item.Stock);
                }
            }
        }

        static void QueryingWithLike()
        {
            using (var db = new Northwind())
            {
                var loggerFactory = db.GetService<ILoggerFactory>();
                loggerFactory.AddProvider(new ConsoleLoggerProvider());

                Write("Enter part of a product name: ");
                string input = ReadLine();

                IQueryable<Product> prods = db.Products
                                              .Where(p => EF.Functions.Like(p.ProductName, $"%{input}%"));

                foreach (Product item in prods)
                {
                    WriteLine("{0} has {1} units in stock. Discountinued? {2}", item.ProductName, item.Stock, item.Discontinued);
                }
            }
        }

        static bool AddProduct(int categoryID, string productName, float? price)
        {
            using (var db = new Northwind())
            {
                var newProduct = new Product
                {
                    CategoryID = categoryID,
                    ProductName = productName,
                    Cost = price
                };
                // mark product as added in change tracking
                db.Products.Add(newProduct);

                // save tracked chnage to database
                int affected = db.SaveChanges();
                return (affected == 1);
            }
           
        }

        static void ListProducts()
        {
            using (var db = new Northwind())
            {
                WriteLine("{0, -3} {1, -35} {2, 8} {3, 5} {4}", "ID", "Product Name", "Cost", "Stock", "Disc.");
                foreach (var item in db.Products.OrderByDescending(p => p.Cost))
                {
                    WriteLine("{0:000} {1, -35} {2,8:$#,##0.00} {3,5} {4}", item.ProductID, item.ProductName, item.Cost,
                    item.Stock, item.Discontinued);
                }
            }
        }

        static bool IncreaseProductPrice(string name, float amount)
        {
            using(var db = new Northwind())
            {
                // get first product whose name starts with name 
                Product updateProduct = db.Products.First(p => p.ProductName.StartsWith(name));
                updateProduct.Cost += amount;

                int affected = db.SaveChanges();
                return (affected == 1);
            }
        }

        static  int DeleteProducts (string name)
        {
            using (var db = new Northwind())
            {
                using (IDbContextTransaction t = db.Database.BeginTransaction())
                {
                    WriteLine("Transaction isolation level: {0}", t.GetDbTransaction().IsolationLevel);

                    var products = db.Products.Where(p => p.ProductName.StartsWith(name));
                    db.Products.RemoveRange(products);

                    int affected = db.SaveChanges();
                    t.Commit();
                    return affected;
                }
                // IEnumerable<Product> products = db.Products.Where(p => p.ProductName.StartsWith(name));
                // db.Products.RemoveRange(products);

                // int affected = db.SaveChanges();
                // return affected;
            }
        }
           
    }
}
