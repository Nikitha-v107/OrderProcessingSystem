using Order.Domain.Enums;
using Newtonsoft.Json;

namespace Order.Domain.DTOs
{
    public class OrderCreatedDto
    {
        [JsonProperty("id")]
        public required string id { get; set; }
        public required string CustomerName { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal TotalAmount { get; set; }
        public required OrderStatus Status { get; set; }
        public required DateTime CreatedAtUtc { get; set; }
    }
}
