using BookStoreApi.Enums;
using BookStoreApi.Models;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var users = new List<User>
        {
            new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = UserRole.Admin },
            new User { Username = "manager", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), Role = UserRole.Manager },
            new User { Username = "user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"), Role = UserRole.User }
        };
        context.Users.AddRange(users);

        var products = new List<Product>
        {
            new Product { Title = "Clean Code", Author = "Robert C. Martin", Price = 29.99m, Stock = 100, ImageUrl = "https://m.media-amazon.com/images/I/41jEbK-jG+L.jpg" },
            new Product { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Price = 39.99m, Stock = 50, ImageUrl = "https://m.media-amazon.com/images/I/41as+WafrFL._SX258_BO1,204,203,200_.jpg" },
            new Product { Title = "Design Patterns", Author = "Erich Gamma", Price = 49.99m, Stock = 30, ImageUrl = "https://m.media-amazon.com/images/I/51kY5Pz2TML._SX258_BO1,204,203,200_.jpg" }
        };
        context.Products.AddRange(products);

        context.SaveChanges();
    }
}
