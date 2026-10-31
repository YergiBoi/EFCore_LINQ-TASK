using EFCore_LINQ_TASK.Enums; // ✅ Add this line
using System;
using System.Collections.Generic;

namespace EFCore_LINQ_TASK.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}