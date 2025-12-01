using Order.Domain.DTOs;
using Order.Domain.Models;

namespace Order.Infrastructure.Services
{
    public interface IOrderService
    {
        Task<Order.Domain.Models.Order> CreateOrderAsync(CreateOrderDto dto);
        Task<Order.Domain.Models.Order?> GetOrderByIdAsync(string orderId);
        Task<List<Order.Domain.Models.Order>> GetAllOrdersAsync();
    }
}
