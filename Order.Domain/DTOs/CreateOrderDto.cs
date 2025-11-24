namespace Order.Domain.DTOs
{
    public class CreateOrderDto
    {
        public required string CustomerName { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required decimal TotalAmount { get; set; }
    }
}
