using System.ComponentModel.DataAnnotations;

namespace API.Entity;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string? CustomerId { get; set; }
    public AppUser User { get; set; } = null!;

    [Range(1, 5)]
    public int Point { get; set; }
    [StringLength(500)]
    public string Comment { get; set; } = null!;
    public DateTime CommentDate { get; set; } = DateTime.Now;
}
  public class ReviewCreateModel
    {
        public int ProductId { get; set; }
        public int Point { get; set; }
        public string Comment { get; set; } = null!;
    }

    public class ReviewEditModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Point { get; set; }
        public string Comment { get; set; } = null!;
    }

    public class ReviewDetail
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string? CustomerId { get; set; }
    public AppUser User { get; set; } = null!;

    [Range(1, 5)]
    public int Point { get; set; }
    [StringLength(500)]
    public string Comment { get; set; } = null!;
    public DateTime CommentDate { get; set; } = DateTime.Now;
}