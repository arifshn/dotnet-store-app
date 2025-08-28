using API.Data;
using API.DTO;
using API.Entity;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;

    private readonly DataContext _context;

    public AccountController(UserManager<AppUser> usermanager, TokenService tokenService, DataContext context)
    {
        _userManager = usermanager;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user == null)
        {
            return BadRequest(new ProblemDetails { Title = "Username hatalı" });
        }
        var result = await _userManager.CheckPasswordAsync(user, model.Password);

        if (result)
        {
            var userCart = await GetOrCreate(model.UserName);
            var cookieCart = await GetOrCreate(Request.Cookies["customerId"]!);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            if (userCart != null)
            {
                foreach (var item in userCart.CartItems)
                {
                    cookieCart.AddItem(item.Product, item.Quantity);
                }
                _context.Carts.Remove(userCart);
            }

            cookieCart.CustomerId = model.UserName;
            await _context.SaveChangesAsync();

            return Ok(new UserDTO
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name!,
                Token = await _tokenService.GenerateToken(user),
                Role = role
            });
        }
        return Unauthorized();
    }

    private async Task<Cart> GetOrCreate(string custId)
    {
        var cart = await _context.Carts
        .Include(i => i.CartItems)
        .ThenInclude(i => i.Product)
        .Where(i => i.CustomerId == custId)
        .FirstOrDefaultAsync();

        if (cart == null)
        {
            var customerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(customerId))
            {
                customerId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddMonths(1),
                    IsEssential = true
                };
                Response.Cookies.Append("customerId", customerId, cookieOptions);
            }
            cart = new Cart { CustomerId = customerId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        return cart;
    }

    [HttpPost("register")]
    public async Task<IActionResult> CreateUser(RegisterDTO model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                               .SelectMany(v => v.Errors)
                               .Select(e => e.ErrorMessage));
            Console.WriteLine(errors);
            return BadRequest(ModelState);
        }
        var user = new AppUser
        {
            Name = model.Name,
            UserName = model.UserName,
            Email = model.Email
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "customer");
            return StatusCode(201);
        }
        return BadRequest(result.Errors);
    }

    [Authorize]
    [HttpGet("getUser")]
    public async Task<ActionResult<UserDTO>> getUser()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name!);
        if (user == null)
        {
            return BadRequest(new ProblemDetails { Title = "Username ya da parola hatalı" });
        }
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.Name!,
            Token = await _tokenService.GenerateToken(user),
            Role = role
        };
    }

[Authorize]
[HttpPut("updateUser")]
public async Task<IActionResult> updateUser(UpdateUserDTO model)
{
    var user = await _userManager.FindByNameAsync(User.Identity?.Name!);
    if (user == null) return NotFound();

    user.Name = model.Name ?? user.Name;

    if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
    {
        var emailResult = await _userManager.SetEmailAsync(user, model.Email);
        if (!emailResult.Succeeded)
            return BadRequest(emailResult.Errors);
    }

    var result = await _userManager.UpdateAsync(user);
    if (result.Succeeded)
    {
        return NoContent();
    }
    return BadRequest(result.Errors);
}


    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();
        if (model.NewPassword != model.NewPasswordRestart)
        {
            return BadRequest(new ProblemDetails { Title = "Yeni parola ve yeni parola tekrarı eşleşmiyor." });
        }
        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            return NoContent();
        }
        return BadRequest(result.Errors);

    }
}