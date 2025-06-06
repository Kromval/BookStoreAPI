namespace BookStoreApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
    }

}
