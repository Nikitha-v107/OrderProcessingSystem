using Order.Domain.Enums;

namespace Order.Domain.Models
{
    public class Order
    {
        public required string Id { get; set; } = Guid.NewGuid().ToString();
        public required string CustomerName { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal TotalAmount { get; set; }
        public required OrderStatus Status { get; set; } = OrderStatus.Created; // "Created", "Processed"
        public required DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
