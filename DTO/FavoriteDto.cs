using API.Entity;

public class FavoriteDTO
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}