using System.Text.Json.Serialization;

namespace API.Entity;

public class Category
{
    public int Id { get; set; }

    public string KategoriAdi { get; set; } = null!;

    public string Url { get; set; } = null!;

    [JsonIgnore]
    public List<Product> Products { get; set; } = new();
}