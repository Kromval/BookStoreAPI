using BookStoreApi.Enums;

namespace BookStoreApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public User? User { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

}
