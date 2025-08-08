namespace API.Entity;

public class Favorite
{
    public int Id { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}