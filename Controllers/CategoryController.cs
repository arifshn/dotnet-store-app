using API.Data;
using API.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly DataContext _context;

    public CategoryController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategory()
    {
        var categories = await _context.Categories.Include(p => p.Products).ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(p => p.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(Category category)
    {

        if (category == null)
        {
            return BadRequest();
         }
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category updatedCategory)
    {
        if (id != updatedCategory.Id)
            return BadRequest("ID eşleşmiyor");

        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        category.KategoriAdi = updatedCategory.KategoriAdi;
        category.Url = updatedCategory.Url;

        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
