using System.ComponentModel.DataAnnotations;

namespace Order.Domain.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Customer name must be between 3 and 100 characters.")]
        public required string CustomerName { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters.")]
        public required string ProductName { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public required int Quantity { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Total amount must be greater than 0.")]
        public required decimal TotalAmount { get; set; }
    }
}
