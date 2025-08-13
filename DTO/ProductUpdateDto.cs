using System.ComponentModel.DataAnnotations;

public class ProductUpdateDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
    [Required]
    public int CategoryId { get; set; }
}
