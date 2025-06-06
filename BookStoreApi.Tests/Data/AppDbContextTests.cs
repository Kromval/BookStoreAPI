using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BookStoreApi.Models;
using BookStoreApi.Enums;

namespace BookStoreApi.Tests.Data
{
    public class AppDbContextTests : IDisposable
    {
        private readonly AppDbContext _context;

        public AppDbContextTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Model_ShouldConvert_OrderStatus_And_UserRole_ToString()
        {
            var model = _context.Model;

            var orderEntity = model.FindEntityType(typeof(Order));
            var statusProp = orderEntity.FindProperty(nameof(Order.Status));
            Assert.NotNull(statusProp.GetValueConverter());
            Assert.Equal(typeof(string), statusProp.GetValueConverter().ProviderClrType);

            var userEntity = model.FindEntityType(typeof(User));
            var roleProp = userEntity.FindProperty(nameof(User.Role));
            Assert.NotNull(roleProp.GetValueConverter());
            Assert.Equal(typeof(string), roleProp.GetValueConverter().ProviderClrType);
        }

        //[Fact]
        //public void Model_ShouldDefine_ForeignKeys_Correctly()
        //{
        //    var model = _context.Model;

        //    var orderFk = model
        //        .FindEntityType(typeof(Order))
        //        .GetForeignKeys()
        //        .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(User));
        //    Assert.NotNull(orderFk);
        //    Assert.Equal(nameof(Order.UserId), orderFk.Properties.Single().Name);

        //    var orderItemFk = model
        //        .FindEntityType(typeof(OrderItem))
        //        .GetForeignKeys()
        //        .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Order));
        //    Assert.NotNull(orderItemFk);
        //    Assert.Equal(nameof(OrderItem.OrderId), orderItemFk.Properties.Single().Name);

        //    var productFk = model
        //        .FindEntityType(typeof(OrderItem))
        //        .GetForeignKeys()
        //        .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Product));
        //    Assert.NotNull(productFk);
        //    Assert.Equal(nameof(OrderItem.ProductId), productFk.Properties.Single().Name);
        //}

        [Fact]
        public void Can_Add_And_Retrieve_Entities_With_Relationships()
        {
            var user = new User { Username = "u1", Role = UserRole.User, PasswordHash = "h" };
            var product = new Product { Title = "P1", Author="A1", Price = 9.99m };
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.SaveChanges();

            var order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = product.Id, Quantity = 2 }
                }
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var fetched = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Single(o => o.Id == order.Id);

            Assert.Equal(user.Id, fetched.UserId);
            Assert.Equal(user.Username, fetched.User.Username);
            Assert.Single(fetched.Items);

            var item = fetched.Items.Single();
            Assert.Equal(product.Id, item.ProductId);
            Assert.Equal(product.Title, item.Product.Title);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public void DbSets_ShouldBe_Queryable()
        {
            Assert.IsAssignableFrom<IQueryable<User>>(_context.Users);
            Assert.IsAssignableFrom<IQueryable<Product>>(_context.Products);
            Assert.IsAssignableFrom<IQueryable<Order>>(_context.Orders);
            Assert.IsAssignableFrom<IQueryable<OrderItem>>(_context.OrderItems);
        }
    }
}
