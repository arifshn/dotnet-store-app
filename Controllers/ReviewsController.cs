using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly DataContext _context;

    public ReviewsController(UserManager<AppUser> userManager, DataContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview([FromBody] ReviewCreateModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized(new { error = "Kullanıcı oturumu doğrulanamadı. Lütfen tekrar giriş yapın." });
        Console.WriteLine($"DEBUG: Current User ID: {User.Identity?.Name}");
        Console.WriteLine($"DEBUG: Product ID for Review: {model.ProductId}");

        if (!ModelState.IsValid || model.Point < 1 || model.Point > 5 || string.IsNullOrWhiteSpace(model.Comment))
            return BadRequest(new { error = "Lütfen geçerli bir puan (1-5) ve yorum girin." });

        var canComment = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == User.Identity!.Name)
            .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == model.ProductId));

        Console.WriteLine($"DEBUG: Can User Comment on Product (ID: {model.ProductId})? {canComment}");


        if (!canComment)
            return StatusCode(403, new { error = "Bu ürünü satın almadığınız için yorum yapamazsınız." });


        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == model.ProductId && r.CustomerId == currentUser.Id);

        if (existingReview != null)
        {
            existingReview.Point = model.Point;
            existingReview.Comment = model.Comment;
            existingReview.CommentDate = DateTime.Now;
            _context.Reviews.Update(existingReview);
        }
        else
        {
            var newReview = new Review
            {
                ProductId = model.ProductId,
                Point = model.Point,
                Comment = model.Comment,
                CommentDate = DateTime.Now,
                CustomerId = currentUser.Id
            };
            _context.Reviews.Add(newReview);
        }

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { message = "Yorumunuz başarıyla kaydedildi." });
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DbUpdateException in AddReview: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { error = "Yorum veritabanına kaydedilirken bir hata oluştu. Lütfen tekrar deneyin." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception in AddReview: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { error = "Yorum kaydedilirken beklenmedik bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return NotFound(new { error = "Yorum bulunamadı." });

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Yorum başarıyla silindi." });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetReview(int id, [FromQuery] int productId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && r.ProductId == productId);

        if (review == null)
            return NotFound(new { error = "Yorum bulunamadı." });

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || (review.CustomerId != User.Identity?.Name && !await _userManager.IsInRoleAsync(currentUser, "Admin")))
            return Forbid();

        var result = new ReviewEditModel
        {
            Id = review.Id,
            ProductId = review.ProductId,
            Point = review.Point,
            Comment = review.Comment
        };

        return Ok(result);
    }
    [HttpGet("product/{productId}")] 
    public async Task<ActionResult<IEnumerable<ReviewDetail>>> GetReviewsForProduct(int productId)
    {
    var reviews = await _context.Reviews
        .Where(r => r.ProductId == productId)
        .Include(r => r.User) 
        .OrderByDescending(r => r.CommentDate) 
        .Select(r => new ReviewDetail 
        {
            Id = r.Id,
            ProductId = r.ProductId,
            Point = r.Point,
            Comment = r.Comment,
            CommentDate = r.CommentDate,
            CustomerId = r.CustomerId,
        })
        .ToListAsync();

        return Ok(reviews);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> EditReview(int id, [FromBody] ReviewEditModel model)
    {
        if (id != model.Id)
            return BadRequest(new { error = "Review ID uyuşmuyor." });

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized(new { error = "Kullanıcı oturumu doğrulanamadı." });

        if (!ModelState.IsValid || model.Point < 1 || model.Point > 5 || string.IsNullOrWhiteSpace(model.Comment))
            return BadRequest(new { error = "Lütfen geçerli bir puan (1-5) ve yorum girin." });

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.ProductId == model.ProductId);

        if (review == null)
            return NotFound(new { error = "Yorum bulunamadı." });

        if (review.CustomerId != User.Identity?.Name && !await _userManager.IsInRoleAsync(currentUser, "admin"))
            return Forbid();

        review.Point = model.Point;
        review.Comment = model.Comment;
        review.CommentDate = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { message = "Yorumunuz başarıyla güncellendi." });
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DbUpdateException in EditReview: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { error = "Yorum veritabanında güncellenirken bir hata oluştu. Lütfen tekrar deneyin." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception in EditReview: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { error = "Yorum güncellenirken beklenmedik bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
    }
}

