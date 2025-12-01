using Order.Domain.Enums;
using Newtonsoft.Json;

namespace Order.Domain.Models
{
    public class Order
    {
        [JsonProperty("id")]
        public required string id { get; set; } = Guid.NewGuid().ToString();
        public required string CustomerName { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal TotalAmount { get; set; }
        public required OrderStatus Status { get; set; } = OrderStatus.Created;
        public required DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
