using Order.Domain.Enums;

namespace Order.Domain.DTOs
{
    public class OrderDto
    {
        public required string Id { get; set; }
        public required string CustomerName { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal TotalAmount { get; set; }
        public required OrderStatus Status { get; set; }
        public required DateTime CreatedAtUtc { get; set; }
    }
}
