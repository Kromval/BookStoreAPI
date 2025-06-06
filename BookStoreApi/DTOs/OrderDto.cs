namespace BookStoreApi.DTOs
{
    public class OrderDto
    {
        public required List<OrderItemDto> Items { get; set; }
    }

}
