using API.DTO;
using API.Entity;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;

    public AccountController(UserManager<AppUser> usermanager, TokenService tokenService)
    {
        _userManager = usermanager;
        _tokenService = tokenService;
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
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            return Ok(new UserDTO
            {
                Name = user.Name!,
                Token = await _tokenService.GenerateToken(user),
                Role = role
            });
        }
        return Unauthorized();
    }

    [HttpPost("register")]
    public async Task<IActionResult> CreateUser(RegisterDTO model)
    {
        if (!ModelState.IsValid)
        {
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
            Name = user.Name!,
            Token = await _tokenService.GenerateToken(user),
            Role = role
        };
    }
}