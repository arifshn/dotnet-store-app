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
    public async Task<IActionResult> GetProducts(
        [FromQuery] int? categoryId = null,
        [FromQuery] string search = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
    [FromQuery] bool showInactive = false)
        
    {
        var query = _context.Products
        .Include(p => p.Category)
        .AsQueryable();

    if (!showInactive)
    {
        query = query.Where(p => p.IsActive);
    }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => 
                p.Name!.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower))
            );
        }
        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
            Category =  new CategoryDto
            {
                Id = p.Category.Id,
                KategoriAdi = p.Category.KategoriAdi,
                Url = p.Category.Url
            } 
        }).ToList();
        var response = new
        {
            products = productDtos,
            totalCount = totalCount,
            currentPage = page,
            pageSize = pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
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

        var review = _context.Reviews
                 .Where(r => r.ProductId == id)
                 .Include(r => r.User)
                 .Select(r => new Review
                 {
                     Id = r.Id,
                     Point = r.Point,
                     Comment = r.Comment,
                     CommentDate = r.CommentDate,
                     User = r.User,
                     CustomerId = r.CustomerId
                 }).ToList();
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
        IsActive = dto.Stock > 0 ? dto.IsActive : false,
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
       var isCategoryValid = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
if (!isCategoryValid)
{
    return BadRequest("Geçersiz kategori ID");
}
    if (!ModelState.IsValid || id != dto.Id)
        {
            return BadRequest(ModelState);
        }

    var existingProduct = await _context.Products
    .Include(p => p.Category)
    .FirstOrDefaultAsync(p=> p.Id == id);
    if (existingProduct == null)
    {
        return NotFound();
    }

    existingProduct.Name = dto.Name;
    existingProduct.Description = dto.Description;
    existingProduct.Price = dto.Price;
    existingProduct.Stock = dto.Stock;
    existingProduct.IsActive = dto.Stock > 0 ? dto.IsActive : false;
    existingProduct.ImageUrl = dto.ImageUrl;
    existingProduct.CategoryId = dto.CategoryId;

    _context.Products.Update(existingProduct);
        try
        {
            await _context.SaveChangesAsync();
    await _context.Entry(existingProduct)
    .Reference(p => p.Category)
    .LoadAsync();
}
        catch (DbUpdateException ex)
        {
            Console.WriteLine("Inner exception: " + ex.InnerException?.Message);
            return StatusCode(500, "Veritabanı hatası: " + ex.Message);
        }

    var updatedDto = new ProductDto
    {
        Id = existingProduct.Id,
        Name = existingProduct.Name!,
        Description = existingProduct.Description,
        Price = existingProduct.Price,
        IsActive = existingProduct.IsActive,
        ImageUrl = existingProduct.ImageUrl,
        Stock = existingProduct.Stock,
        CategoryId = existingProduct.CategoryId,
        Category = new CategoryDto
        {
            Id = existingProduct.Category.Id,
            KategoriAdi = existingProduct.Category.KategoriAdi,
            Url = existingProduct.Category.Url
        }
    };

    return Ok(updatedDto);
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