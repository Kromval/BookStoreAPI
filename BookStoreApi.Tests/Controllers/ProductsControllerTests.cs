using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BookStoreApi.Controllers;
using BookStoreApi.Models;

namespace BookStoreApi.Tests.Controllers
{
    public class ProductsControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"ProductsDb_{Guid.NewGuid()}")
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _context.Products.AddRange(
                new Product { Title = "Book A", Author = "Author A", Price = 10.0m, Stock = 5 },
                new Product { Title = "Book B", Author = "Author B", Price = 20.0m, Stock = 3 }
            );
            _context.SaveChanges();

            _controller = new ProductsController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithAllProducts()
        {
            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var products = Assert.IsAssignableFrom<System.Collections.Generic.List<Product>>(okResult.Value);
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithProduct()
        {
            var existing = _context.Products.First();

            var result = await _controller.GetById(existing.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var product = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(existing.Id, product.Id);
            Assert.Equal(existing.Title, product.Title);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            var result = await _controller.GetById(int.MaxValue);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ValidProduct_ReturnsOkAndAddsProduct()
        {
            var newProduct = new Product { Title = "Book C", Author = "Author C", Price = 15.5m, Stock = 7 };

            var result = await _controller.Create(newProduct);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var created = Assert.IsType<Product>(okResult.Value);
            Assert.NotEqual(0, created.Id);
            Assert.Equal("Book C", created.Title);

            var dbProduct = await _context.Products.FindAsync(created.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal("Author C", dbProduct.Author);
        }

        [Fact]
        public async Task Update_ExistingProduct_ReturnsOkAndUpdatesFields()
        {
            var product = _context.Products.First();
            var updatedInfo = new Product
            {
                Title = "Updated Title",
                Author = "Updated Author",
                Price = 99.9m,
                Stock = 1
            };

            var result = await _controller.Update(product.Id, updatedInfo);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var updated = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(product.Id, updated.Id);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Updated Author", updated.Author);
            Assert.Equal(99.9m, updated.Price);
            Assert.Equal(1, updated.Stock);
        }


        [Fact]
        public async Task Delete_ExistingProduct_ReturnsOkAndRemovesProduct()
        {
            var product = _context.Products.First();

            var result = await _controller.Delete(product.Id);

            Assert.IsType<OkResult>(result);
            var dbProduct = await _context.Products.FindAsync(product.Id);
            Assert.Null(dbProduct);
        }

        [Fact]
        public async Task Delete_NonExistingProduct_ReturnsNotFound()
        {
            var result = await _controller.Delete(int.MaxValue);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
