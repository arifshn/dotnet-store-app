namespace dotnet_store.Models
{
    public class ReviewEditModel
    {
        public int Id { get; set; }
        public int UrunId { get; set; }
        public int Point { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}