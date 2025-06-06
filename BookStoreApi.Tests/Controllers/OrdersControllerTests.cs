using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BookStoreApi.Controllers;
using BookStoreApi.DTOs;
using BookStoreApi.Models;
using BookStoreApi.Enums;

namespace BookStoreApi.Tests.Controllers
{
    public class OrdersControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly User _user;
        private readonly Product _product;

        public OrdersControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrdersDb_{Guid.NewGuid()}")
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _user = new User { Username = "testuser", Role = UserRole.User, PasswordHash = "hash" };
            _product = new Product { Title = "Prod1", Author = "Auth", Price = 5.5m, Stock = 10 };
            _context.Users.Add(_user);
            _context.Products.Add(_product);
            _context.SaveChanges();
        }

        private OrdersController CreateController(string username = null)
        {
            var controller = new OrdersController(_context);
            var httpContext = new DefaultHttpContext();
            if (username != null)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            }
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateOrder_ValidUser_CreatesOrderAndReturnsOk()
        {
            // Arrange
            var controller = CreateController(_user.Username);
            var dto = new OrderDto
            {
                Items = new List<OrderItemDto>
        {
            new OrderItemDto { ProductId = _product.Id, Quantity = 2 }
        }
            };

            // Act
            var result = await controller.CreateOrder(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic data = ok.Value;
            Assert.Equal("Order created successfully", (string)data.message);
            int orderId = (int)data.orderId;

            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == orderId);
            Assert.NotNull(order);
            Assert.Equal(_user.Id, order.UserId);
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Single(order.Items);
            Assert.Equal(_product.Id, order.Items[0].ProductId);
            Assert.Equal(2, order.Items[0].Quantity);
        }




        [Fact]
        public async Task GetMyOrders_ValidUser_ReturnsUserOrders()
        {
            // Arrange: создаем заказ для пользователя
            var order = new Order
            {
                UserId = _user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = new System.Collections.Generic.List<OrderItem>
                {
                    new OrderItem { ProductId = _product.Id, Quantity = 3 }
                }
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var controller = CreateController(_user.Username);

            // Act
            var result = await controller.GetMyOrders();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<dynamic>>(ok.Value);
            var orders = list.ToList();
            Assert.Single(orders);
            dynamic fetched = orders[0];
            Assert.Equal(order.Id, (int)fetched.Id);
            Assert.Equal(OrderStatus.Pending, fetched.Status);
            var items = ((System.Collections.Generic.IEnumerable<dynamic>)fetched.Items).ToList();
            Assert.Single(items);
            Assert.Equal(_product.Title, (string)items[0].ProductTitle);
            Assert.Equal(3, (int)items[0].Quantity);
        }

        [Fact]
        public async Task GetMyOrders_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange: no user
            var controller = CreateController("unknown");

            // Act
            var result = await controller.GetMyOrders();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAllOrders_ReturnsAllOrders()
        {
            // Arrange: создаем два заказа от разных пользователей
            var otherUser = new User { Username = "u2", Role = UserRole.User, PasswordHash = "h" };
            _context.Users.Add(otherUser);
            _context.SaveChanges();

            var order1 = new Order { UserId = _user.Id, CreatedAt = DateTime.UtcNow, Status = OrderStatus.Pending, Items = new System.Collections.Generic.List<OrderItem>() };
            var order2 = new Order { UserId = otherUser.Id, CreatedAt = DateTime.UtcNow, Status = OrderStatus.Delivered, Items = new System.Collections.Generic.List<OrderItem>() };
            _context.Orders.AddRange(order1, order2);
            _context.SaveChanges();

            var controller = CreateController();

            // Act
            var result = await controller.GetAllOrders();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var orders = Assert.IsAssignableFrom<System.Collections.Generic.List<Order>>(ok.Value);
            Assert.Equal(2, orders.Count);
        }

        //[Fact]
        //public async Task UpdateOrderStatus_ExistingOrder_UpdatesStatus()
        //{
        //    // Arrange
        //    var order = new Order { UserId = _user.Id, CreatedAt = DateTime.UtcNow, Status = OrderStatus.Pending, Items = new System.Collections.Generic.List<OrderItem>() };
        //    _context.Orders.Add(order);
        //    _context.SaveChanges();

        //    var controller = CreateController();
        //    var dto = new UpdateOrderStatusDto { Status = OrderStatus.Shipped.ToString() };

        //    // Act
        //    var result = await controller.UpdateOrderStatus(order.Id, dto);

        //    // Assert
        //    var ok = Assert.IsType<OkObjectResult>(result);
        //    dynamic data = ok.Value;
        //    Assert.Contains("Order status updated to Shipped", (string)data.message);

        //    var updated = _context.Orders.Find(order.Id);
        //    Assert.Equal(OrderStatus.Shipped, updated.Status);
        //}

        [Fact]
        public async Task UpdateOrderStatus_NonExistingOrder_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController();
            var dto = new UpdateOrderStatusDto { Status = OrderStatus.Shipped.ToString() };

            // Act
            var result = await controller.UpdateOrderStatus(int.MaxValue, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
