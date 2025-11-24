using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Order.Domain.DTOs;
using Order.Infrastructure.Settings;
using System.Text.Json;

namespace Order.Infrastructure.EventGrid
{
    public class OrderEventPublisher : IOrderEventPublisher
    {
        private readonly EventGridPublisherClient _publisherClient;

        public OrderEventPublisher(IOptions<EventGridSettings> settings)
        {
            _publisherClient = new EventGridPublisherClient(
                new Uri(settings.Value.TopicEndpoint),
                new Azure.AzureKeyCredential(settings.Value.TopicKey));
        }

        public async Task PublishOrderCreatedEventAsync(OrderCreatedDto order)
        {
            var eventData = new EventGridEvent(
                subject: $"/order/{order.Id}",
                eventType: "Order.Created",
                dataVersion: "1.0",
                data: order);

            await _publisherClient.SendEventAsync(eventData);
        }
    }
}
