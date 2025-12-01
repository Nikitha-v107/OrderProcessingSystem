using Azure.Messaging.EventGrid;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Order.Domain.DTOs;
using Order.Infrastructure.Repositories;
using Order.Infrastructure.Settings;
using System.Text.Json;
using Order.Domain.Enums;

namespace Order.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly EventProcessorClient _processor;

    public Worker(ILogger<Worker> logger, IOrderRepository orderRepository, Microsoft.Extensions.Options.IOptions<EventHubSettings> eventHubSettings, Microsoft.Extensions.Options.IOptions<CosmosDbSettings> cosmosDbSettings)
    {
        _logger = logger;
        _orderRepository = orderRepository;

        var blobContainerClient = new BlobContainerClient(
            eventHubSettings.Value.BlobStorageConnectionString,
            eventHubSettings.Value.BlobContainerName);

        _processor = new EventProcessorClient(
            blobContainerClient,
            eventHubSettings.Value.ConsumerGroup,
            eventHubSettings.Value.ConnectionString,
            eventHubSettings.Value.EventHubName);

        _processor.ProcessEventAsync += ProcessEventHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        try
        {
            var eventGridEvents = EventGridEvent.ParseMany(eventArgs.Data.EventBody);

            if (eventGridEvents == null)
            {
                _logger.LogWarning("Received null or empty Event Grid events array.");
                return;
            }

            foreach (var eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.EventType == "Order.Created")
                {
                    var orderCreatedDto = eventGridEvent.Data.ToObjectFromJson<OrderCreatedDto>();
                    if (orderCreatedDto != null)
                    {
                        _logger.LogInformation("Processing Order.Created event for Order ID: {OrderId}", orderCreatedDto.id);

                        await _orderRepository.UpdateOrderStatusAsync(orderCreatedDto.id, OrderStatus.Processed);
                        _logger.LogInformation("Order ID: {OrderId} status updated to Processed.", orderCreatedDto.id);
                    }
                    else
                    {
                        _logger.LogWarning("Could not deserialize Order.Created event data.");
                    }
                }
            }

            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event from Event Hub.");
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
    {
        _logger.LogError(eventArgs.Exception, "Error in Event Hub processor.");
        return Task.CompletedTask;
    }
}
