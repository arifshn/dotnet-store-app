using API.Data;
using API.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;


[ApiController]
[Route("/api/[controller]")]
public class ProductsController : ControllerBase
{

    private readonly DataContext _context;
    public ProductsController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
     var products = await _context.Products.Include(p => p.Category).ToListAsync();

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name!,
            Description = p.Description,
            Price = p.Price,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            Category = new CategoryDto
            {
                Id = p.Category.Id,
                KategoriAdi = p.Category.KategoriAdi,
                Url = p.Category.Url
            }
        }).ToList();
        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProducts(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
       var product = await _context.Products
    .Include(p => p.Category)
    .FirstOrDefaultAsync(p => p.Id == id);


        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);  
    }

 [HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    var product = new Product
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        IsActive = dto.IsActive,
        ImageUrl = dto.ImageUrl,
        Stock = dto.Stock,
        CategoryId = dto.CategoryId
    };
    _context.Products.Add(product);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
}

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
    {
       
    if (!ModelState.IsValid || id != dto.Id)
    {
        return BadRequest(ModelState);
    }

    var existingProduct = await _context.Products.FindAsync(id);
    if (existingProduct == null)
    {
        return NotFound();
    }

    existingProduct.Name = dto.Name;
    existingProduct.Description = dto.Description;
    existingProduct.Price = dto.Price;
    existingProduct.Stock = dto.Stock;
    existingProduct.IsActive = dto.IsActive;
    existingProduct.ImageUrl = dto.ImageUrl;
    existingProduct.CategoryId = dto.CategoryId;

    _context.Products.Update(existingProduct);
    await _context.SaveChangesAsync();

    return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}