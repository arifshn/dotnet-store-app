using API.Data;
using API.DTO;
using API.Entity;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class FavoriteService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public FavoriteService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FavoriteDTO>> GetFavoritesByUser(string customerId)
    {
        var favorites = await _context.Favorites
                            .Include(f => f.Product)
                            .Where(f => f.CustomerId == customerId)
                            .ToListAsync();

        return _mapper.Map<List<FavoriteDTO>>(favorites);
    }

 public async Task<FavoriteDTO> AddFavorite(FavoriteDTO favoriteDto)
{
    var alreadyExists = await _context.Favorites
        .AnyAsync(f => f.CustomerId == favoriteDto.CustomerId && f.ProductId == favoriteDto.ProductId);

    if (alreadyExists)
        throw new InvalidOperationException("Bu ürün zaten favorilerinizde");

    var favorite = _mapper.Map<Favorite>(favoriteDto);
    favorite.CreatedAt = DateTime.UtcNow;

    _context.Favorites.Add(favorite);
    await _context.SaveChangesAsync();

    return _mapper.Map<FavoriteDTO>(favorite);
}

    public async Task<bool> RemoveFavorite(int id, string customerId)
    {
        var favorite = await _context.Favorites
                        .FirstOrDefaultAsync(f => f.Id == id && f.CustomerId == customerId);
        if (favorite == null) return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }
}
