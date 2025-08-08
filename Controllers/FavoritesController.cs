using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FavoritiesController : ControllerBase
{
    private readonly FavoriteService _favoriteService;

    public FavoritiesController(FavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FavoriteDTO>>> GetFavorites()
    {
        var userId = User.Identity!.Name!;
        if (string.IsNullOrEmpty(userId))
        {
        return Unauthorized();
        }
        var favorites = await _favoriteService.GetFavoritesByUser(userId);
        return Ok(favorites);
    }

   [HttpPost]
    public async Task<ActionResult<FavoriteDTO>> AddFavorite([FromBody] FavoriteDTO favoriteDto)
    {
        var userId = User.Identity!.Name!;
        favoriteDto.CustomerId = userId;

        var addedFavorite = await _favoriteService.AddFavorite(favoriteDto);
        return CreatedAtAction(nameof(GetFavorites), new { id = addedFavorite.Id }, addedFavorite);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        var userId = User.Identity!.Name!;
        var success = await _favoriteService.RemoveFavorite(id, userId);

        if (!success) return NotFound();
        return NoContent();
    }
} 
