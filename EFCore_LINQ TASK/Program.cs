using EFCore_LINQ_TASK.DB;
using EFCore_LINQ_TASK.Models;
using EFCore_LINQ_TASK.Enums;
using System;
using System.Linq;

namespace EFCore_LINQ_TASK
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var db = new OnlineStoreContext();
            if (!db.Customers.Any())
            {
                var cat1 = new Category { Name = "Laptops" };
                var cat2 = new Category { Name = "Accessories" };
                db.Categories.AddRange(cat1, cat2);

                var p1 = new Product { Name = "Gaming Laptop", Price = 1500m, Stock = 5, Category = cat1 };
                var p2 = new Product { Name = "Mouse", Price = 20m, Stock = 50, Category = cat2 };
                db.Products.AddRange(p1, p2);

                var c1 = new Customer { FullName = "Ahmed", Email = "a@a.com", City = "Cairo", RegisteredAt = DateTime.Now.AddDays(-10) };
                var c2 = new Customer { FullName = "Mona", Email = "m@m.com", City = "Alex", RegisteredAt = DateTime.Now.AddDays(-60) };
                db.Customers.AddRange(c1, c2);

                var o1 = new Order { Customer = c1, OrderDate = DateTime.Now, Status = OrderStatus.Delivered };
                var o2 = new Order { Customer = c1, OrderDate = DateTime.Now, Status = OrderStatus.Cancelled };
                var o3 = new Order { Customer = c2, OrderDate = DateTime.Now, Status = OrderStatus.Pending };
                db.Orders.AddRange(o1, o2, o3);

                var oi1 = new OrderItem { Order = o1, Product = p1, Quantity = 2, UnitPrice = 1500m, Discount = 0.10m };
                var oi2 = new OrderItem { Order = o1, Product = p2, Quantity = 5, UnitPrice = 20m, Discount = 0m };
                db.OrderItems.AddRange(oi1, oi2);

                var r1 = new Review { Product = p1, Customer = c1, Rating = 5, Comment = "Great", CreatedAt = DateTime.Now };
                db.Reviews.Add(r1);

                db.SaveChanges();
                Console.WriteLine("Database seeded.");
            }

            var deliveredOrders = db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Select(o => new
                {
                    OrderId = o.Id,
                    CustomerName = o.Customer.FullName,
                    Total = o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            foreach (var order in deliveredOrders)
            {
                Console.WriteLine($"Order ID: {order.OrderId}, Customer: {order.CustomerName}, Total: {order.Total:C}");
            }

            var totalOrders = db.Orders.Count();

            var ordersPerStatus = db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = totalOrders > 0 ? (g.Count() * 100.0) / totalOrders : 0
                })
                .ToList();

            foreach (var item in ordersPerStatus)
            {
                Console.WriteLine($"Status: {item.Status}, Count: {item.Count}, Percentage: {item.Percentage:F2}%");
            }

            var productsAboveAvg = db.Products
                .Where(p => p.Price > db.Products
                    .Where(p2 => p2.CategoryId == p.CategoryId)
                    .Average(p2 => p2.Price))
                .Select(p => new
                {
                    ProductName = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    CategoryAverage = db.Products.Where(p2 => p2.CategoryId == p.CategoryId).Average(p2 => p2.Price)
                })
                .ToList();

            foreach (var p in productsAboveAvg)
            {
                Console.WriteLine($"Product: {p.ProductName}, Price: {p.Price}, Category: {p.CategoryName}, Avg: {p.CategoryAverage:C}");
            }

            
            var neverOrderedProducts = db.Products
                .Where(p => !p.OrderItems.Any())
                .Select(p => p.Name)
                .ToList();

            Console.WriteLine("\n--- Question 4: Products Never Ordered ---");
            foreach (var productName in neverOrderedProducts)
            {
                Console.WriteLine($"- {productName}");
            }

            
            var recentCustomersWithOrders = db.Customers
                .Where(c => c.RegisteredAt >= DateTime.Now.AddDays(-30) && c.Orders.Any())
                .Select(c => new
                {
                    CustomerName = c.FullName,
                    RegisteredAt = c.RegisteredAt
                })
                .ToList();

            Console.WriteLine("\n--- Question 5: Recent Customers With Orders ---");
            foreach (var c in recentCustomersWithOrders)
            {
                Console.WriteLine($"Customer: {c.CustomerName}, Registered: {c.RegisteredAt:d}");
            }

         
            var customersWithManyCancelled = db.Customers
                .Where(c => c.Orders.Count(o => o.Status == OrderStatus.Cancelled) > 2)
                .Select(c => new
                {
                    CustomerName = c.FullName,
                    CancelledCount = c.Orders.Count(o => o.Status == OrderStatus.Cancelled)
                })
                .ToList();

            Console.WriteLine("\n--- Question 6: Customers With > 2 Cancelled Orders ---");
            foreach (var c in customersWithManyCancelled)
            {
                Console.WriteLine($"Customer: {c.CustomerName}, Cancelled Orders: {c.CancelledCount}");
            }

            var top5Customers = db.Customers
                .Select(c => new
                {
                    CustomerName = c.FullName,
                    DeliveredOrders = c.Orders.Where(o => o.Status == OrderStatus.Delivered).ToList()
                })
                .AsEnumerable()
                .Select(c => new
                {
                    c.CustomerName,
                    OrderCount = c.DeliveredOrders.Count,
                    TotalSpent = c.DeliveredOrders.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))),
                    AverageOrderValue = c.DeliveredOrders.Any()
                        ? c.DeliveredOrders.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))) / c.DeliveredOrders.Count
                        : 0
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToList();

            Console.WriteLine("\n--- Question 7: Top 5 Customers by Spending ---");
            foreach (var c in top5Customers)
            {
                Console.WriteLine($"Customer: {c.CustomerName}, Orders: {c.OrderCount}, Spent: {c.TotalSpent:C}, Avg: {c.AverageOrderValue:C}");
            }

            
            var monthlyReport = db.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.OrderDate.Year == 2025)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Year = 2025,
                    Month = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount)))
                })
                .OrderBy(x => x.Month)
                .ToList();

            Console.WriteLine("\n--- Question 8: 2025 Monthly Report ---");
            foreach (var m in monthlyReport)
            {
                Console.WriteLine($"Month: {m.Month}, Orders: {m.OrderCount}, Revenue: {m.Revenue:C}");
            }

            
            var revenuePerCategory = db.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount))
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            Console.WriteLine("\n--- Question 9: Revenue Per Category ---");
            foreach (var c in revenuePerCategory)
            {
                Console.WriteLine($"Category: {c.CategoryName}, Revenue: {c.Revenue:C}");
            }

           
            var customersNoOrdersByCity = db.Customers
                .Where(c => !c.Orders.Any())
                .GroupBy(c => c.City)
                .Select(g => new
                {
                    City = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            Console.WriteLine("\n--- Question 10: Customers With No Orders By City ---");
            foreach (var c in customersNoOrdersByCity)
            {
                Console.WriteLine($"City: {c.City}, Count: {c.Count}");
            }

            
            var topRatedProducts = db.Products
                .Where(p => p.Reviews.Count >= 10 && p.Reviews.Average(r => r.Rating) >= 4.5)
                .Select(p => new
                {
                    ProductName = p.Name,
                    CategoryName = p.Category.Name,
                    ReviewCount = p.Reviews.Count,
                    AverageRating = p.Reviews.Average(r => r.Rating)
                })
                .ToList();

            Console.WriteLine("\n--- Question 11: Products with >= 10 Reviews and >= 4.5 Rating ---");
            foreach (var p in topRatedProducts)
            {
                Console.WriteLine($"Product: {p.ProductName}, Category: {p.CategoryName}, Reviews: {p.ReviewCount}, Avg: {p.AverageRating:F1}");
            }

            
            var top2PerCategory = db.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    TopProducts = c.Products
                        .OrderByDescending(p => p.Price)
                        .Take(2)
                        .Select(p => p.Name)
                        .ToList()
                })
                .ToList();

            Console.WriteLine("\n--- Question 12: Top 2 Most Expensive Products Per Category ---");
            foreach (var c in top2PerCategory)
            {
                Console.WriteLine($"Category: {c.CategoryName}");
                foreach (var pName in c.TopProducts)
                {
                    Console.WriteLine($"  - {pName}");
                }
            }

            var lowStockProducts = db.Products
                .Select(p => new
                {
                    ProductName = p.Name,
                    Stock = p.Stock,
                    PendingQuantity = p.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Pending)
                        .Sum(oi => oi.Quantity)
                })
                .Where(p => p.Stock < p.PendingQuantity)
                .ToList();

            Console.WriteLine("\n--- Question 13: Low Stock vs Pending Quantity ---");
            foreach (var p in lowStockProducts)
            {
                Console.WriteLine($"Product: {p.ProductName}, Stock: {p.Stock}, Pending Qty: {p.PendingQuantity}");
            }

            var customerSpending = db.Customers
                .Select(c => new
                {
                    CustomerName = c.FullName,
                    TotalSpent = c.Orders
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount)))
                })
                .ToList();

            var avgSpending = customerSpending.Average(c => c.TotalSpent);

            var aboveAvgCustomers = customerSpending
                .Where(c => c.TotalSpent > avgSpending)
                .ToList();

            Console.WriteLine("\n--- Question 14: Customers Above Average Spending ---");
            foreach (var c in aboveAvgCustomers)
            {
                Console.WriteLine($"Customer: {c.CustomerName}, Spent: {c.TotalSpent:C}");
            }

            var bestSellingPerCategory = db.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    BestProduct = c.Products
                        .OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity))
                        .Select(p => p.Name)
                        .FirstOrDefault()
                })
                .ToList();

            Console.WriteLine("\n--- Question 15: Best Selling Product Per Category ---");
            foreach (var c in bestSellingPerCategory)
            {
                Console.WriteLine($"Category: {c.CategoryName}, Best Product: {c.BestProduct ?? "None"}");
            }

            var laptopCategory = db.Categories.FirstOrDefault(c => c.Name == "Laptops");

            if (laptopCategory != null)
            {
                var laptopProductIds = laptopCategory.Products.Select(p => p.Id).ToList();
                var totalLaptopProducts = laptopProductIds.Count;

                var customersBoughtAllLaptops = db.Customers
                    .Where(c => c.Orders
                        .SelectMany(o => o.Items)
                        .Select(oi => oi.ProductId)
                        .Distinct()
                        .Count(pId => laptopProductIds.Contains(pId)) == totalLaptopProducts)
                    .Select(c => c.FullName)
                    .ToList();

                Console.WriteLine("\n--- Question 16: Customers Who Bought All Laptops ---");
                foreach (var name in customersBoughtAllLaptops)
                {
                    Console.WriteLine($"- {name}");
                }
            }
            else
            {
                Console.WriteLine("\n--- Question 16: No 'Laptops' category found in seed data. ---");
            }
        }
       
    }
}