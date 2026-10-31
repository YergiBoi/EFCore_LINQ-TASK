using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore_LINQ_TASK.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Product> Products { get; set; } = new();
}