using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApi.Models;
using BookStoreApi.DTOs;
using BookStoreApi.Controllers;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreApi.Tests.Controllers
{
    public class UsersControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb_UsersController")
                .Options;
            _context = new AppDbContext(options);
            _controller = new UsersController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfUsers()
        {
            // Arrange
            _context.Users.Add(new User { Username = "user1", PasswordHash = "hashedPassword" });
            _context.Users.Add(new User { Username = "user2", PasswordHash = "hashedPassword" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<System.Collections.Generic.List<User>>(okResult.Value);
            Assert.Equal(2, users.Count);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            var user = new User { Username = "user1", PasswordHash = "hashedPassword" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetById(user.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(user.Id, returnUser.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.GetById(999); // non-existing ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsOkResult_WhenUserIsCreated()
        {
            // Arrange
            var dto = new RegisterDto { Username = "user1", Password = "password" };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(dto.Username, createdUser.Username);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WhenUserIsUpdated()
        {
            // Arrange
            var user = new User { Username = "user1", PasswordHash = "hashedPassword" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new RegisterDto { Username = "updatedUser", Password = "newPassword" };

            // Act
            var result = await _controller.Update(user.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(dto.Username, updatedUser.Username);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.Update(999, new RegisterDto { Username = "nonExisting", Password = "password" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsOkResult_WhenUserIsDeleted()
        {
            // Arrange
            var user = new User { Username = "user1", PasswordHash = "hashedPassword" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(user.Id);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(_context.Users.Find(user.Id)); // user should be deleted
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.Delete(999); // non-existing ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
