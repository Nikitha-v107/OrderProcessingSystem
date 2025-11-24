using Order.Domain.DTOs;

namespace Order.Infrastructure.EventGrid
{
    public interface IOrderEventPublisher
    {
        Task PublishOrderCreatedEventAsync(OrderCreatedDto order);
    }
}
