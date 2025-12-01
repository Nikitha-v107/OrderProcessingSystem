using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Order.Domain.DTOs;
using Order.Infrastructure.Settings;
using System.Text.Json;
using Order.Domain.Enums;
using Microsoft.Extensions.Logging;
using Order.Domain.Exceptions;

namespace Order.Infrastructure.EventGrid
{
    public class OrderEventPublisher : IOrderEventPublisher
    {
        private readonly EventGridPublisherClient _publisherClient;
        private readonly ILogger<OrderEventPublisher> _logger;

        public OrderEventPublisher(IOptions<EventGridSettings> settings, ILogger<OrderEventPublisher> logger)
        {
            _publisherClient = new EventGridPublisherClient(
                new Uri(settings.Value.TopicEndpoint),
                new Azure.AzureKeyCredential(settings.Value.TopicKey));
            _logger = logger;
        }

        public async Task PublishOrderCreatedEventAsync(OrderCreatedDto order)
        {
            _logger.LogInformation("Attempting to publish Order.Created event for Order ID: {OrderId}", order.id);
            if (order == null)
            {
                throw new ValidationException("Order cannot be null while publishing event.");
            }
            try
            {
                var eventData = new EventGridEvent(
                    subject: $"/order/{order.id}",
                    eventType: "Order.Created",
                    dataVersion: "1.0",
                    data: order);

                await _publisherClient.SendEventAsync(eventData);
                _logger.LogInformation("Order.Created event published successfully for Order ID: {OrderId}", order.id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing Order.Created event for Order ID: {OrderId}.", order.id);
                throw new InfrastructureException($"Failed to publish Order.Created event for Order ID: {order.id}.", ex);
            }
        }
    }
}
