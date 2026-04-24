using Microsoft.EntityFrameworkCore;
using InventoryMaster.Api.Models;

namespace InventoryMaster.Api.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}