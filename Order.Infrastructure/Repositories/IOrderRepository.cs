using Order.Domain.Models;

namespace Order.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<Order.Domain.Models.Order> GetOrderAsync(string id);
        Task<IEnumerable<Order.Domain.Models.Order>> GetOrdersAsync();
        Task AddOrderAsync(Order.Domain.Models.Order order);
        Task UpdateOrderStatusAsync(string id, string status);
    }
}
