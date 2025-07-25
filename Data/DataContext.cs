using API.Entity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            
          new List<Product>
          {
            new Product{
                Id =1,
                Name="deneme1",
                Description="açıklama",
                ImageUrl="1.jpg",
                Price=70000,
                IsActive=true,
                Stock=100
            },
            new Product{
                Id =2,
                Name="deneme2",
                Description="açıklama",
                ImageUrl="1.jpg",
                Price=80000,
                IsActive=true,
                Stock=100
            },
            new Product{
                Id =3,
                Name="deneme3",
                Description="açıklama",
                ImageUrl="1.jpg",
                Price=90000,
                IsActive=true,
                Stock=100
            },
            new Product{
                Id =4,
                Name="deneme4",
                Description="açıklama",
                ImageUrl="1.jpg",
                Price=10000,
                IsActive=true,
                Stock=100
            },
          }
        );
    }
}