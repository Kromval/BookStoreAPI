using BookStoreApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookStoreApi.DTOs;
using BookStoreApi.Enums;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

   
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder(OrderDto orderDto)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();

            var order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = orderDto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created successfully", orderId = order.Id });
        }

        // Получить свои заказы
        [HttpGet("my")]
        [Authorize(Roles = "User,Admin,Manager")]
        public async Task<IActionResult> GetMyOrders()
        {
            var username = User.Identity.Name;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.User.Id == user.Id)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.Status,
                Items = o.Items.Select(i => new
                {
                    ProductTitle = i.Product.Title,
                    Quantity = i.Quantity
                }).ToList()
            });

            return Ok(result);
        }


        // Получить все заказы (для менеджеров и админов)
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            return Ok(orders);
        }

        // Обновить статус заказа
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto statusDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = Enum.Parse<OrderStatus>(statusDto.Status);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Order status updated to {statusDto.Status}" });
        }
    }
}
