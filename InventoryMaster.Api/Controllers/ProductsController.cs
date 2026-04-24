using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryMaster.Api.Data;
using InventoryMaster.Api.Models;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public ProductsController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        // Lógica de negocio: El stock no puede ser menor a cero
        if (product.Stock < 0)
        {
            return BadRequest("El stock inicial no puede ser negativo.");
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        // Incluimos la categoría para que la respuesta sea completa
        return await _context.Products.Include(p => p.Category).ToListAsync();
    }

    // GET: api/products/low-stock
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<Product>>> GetLowStock()
    {
        // Solo traeremos los productos que tengan 5 o menos unidades
        return await _context.Products
            .Where(p => p.Stock <= 5)
            .Include(p => p.Category)
            .ToListAsync();
    }

    [HttpPost("sell")]
    public async Task<IActionResult> SellProduct([FromBody] SellProductDto sellDto)
    {
        // 1. Buscar el producto en la base de datos
        var product = await _context.Products.FindAsync(sellDto.ProductId);

        if (product == null)
        {
            return NotFound("El producto no existe.");
        }

        // 2. Verificar si hay suficiente stock
        if (product.Stock < sellDto.Quantity)
        {
            return BadRequest($"Stock insuficiente. Solo quedan {product.Stock} unidades.");
        }

        // 3. Lógica de venta: Restar el stock
        product.Stock -= sellDto.Quantity;

        // 4. Guardar los cambios
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Venta realizada con éxito",
            productName = product.Name,
            remainingStock = product.Stock
        });
    }

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // 1. Buscamos el producto en la base de datos
        var product = await _context.Products.FindAsync(id);

        // 2. Si no existe, avisamos al usuario (Evita errores 500)
        if (product == null)
        {
            return NotFound($"No se encontró el producto con ID {id}");
        }

        // 3. Marcamos el producto para ser eliminado
        _context.Products.Remove(product);

        // 4. Aplicamos los cambios en SQL Server
        await _context.SaveChangesAsync();

        return Ok(new { message = $"El producto '{product.Name}' ha sido eliminado exitosamente." });
    }

    // GET: api/Products/search/dell
    [HttpGet("search/{name}")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchByName(string name)
    {
        // Usamos .Contains() para buscar coincidencias en cualquier parte del nombre
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Name.Contains(name))
            .ToListAsync();

        if (products == null || !products.Any())
        {
            return NotFound($"No se encontraron productos que coincidan con: {name}");
        }

        return Ok(products);
    }
}

