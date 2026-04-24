namespace InventoryMaster.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Relación: Una categoría tiene muchos productos
    public List<Product> Products { get; set; } = new();
}