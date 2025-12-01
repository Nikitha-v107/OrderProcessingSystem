using Order.Domain.Models;
using Order.Domain.Enums;

namespace Order.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<Order.Domain.Models.Order?> GetOrderAsync(string orderId);
        Task<IEnumerable<Order.Domain.Models.Order>> GetOrdersAsync();
        Task AddOrderAsync(Order.Domain.Models.Order order);
        Task UpdateOrderStatusAsync(string orderId, OrderStatus status);
    }
}
