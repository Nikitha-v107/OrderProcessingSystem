using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Order.Domain.Models;
using Order.Infrastructure.Settings;
using Order.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Order.Domain.Exceptions;

namespace Order.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly Container _container;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(CosmosClient cosmosClient, IOptions<CosmosDbSettings> settings, ILogger<OrderRepository> logger)
        {
            _container = cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.ContainerName);
            _logger = logger;
        }

        public async Task<Order.Domain.Models.Order?> GetOrderAsync(string orderId)
        {
            _logger.LogInformation("Attempting to retrieve order by ID: {OrderId} from Cosmos DB.", orderId);
            try
            {
                ItemResponse<Order.Domain.Models.Order> response = await _container.ReadItemAsync<Order.Domain.Models.Order>(orderId, new PartitionKey(orderId));
                _logger.LogInformation("Order ID: {OrderId} retrieved successfully from Cosmos DB.", orderId);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order ID: {OrderId} not found in Cosmos DB.", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order ID: {OrderId} from Cosmos DB.", orderId);
                throw new InfrastructureException($"Failed to retrieve order ID: {orderId} from Cosmos DB.", ex);
            }
        }

        public async Task<IEnumerable<Order.Domain.Models.Order>> GetOrdersAsync()
        {
            _logger.LogInformation("Attempting to retrieve all orders from Cosmos DB.");
            try
            {
                var query = _container.GetItemQueryIterator<Order.Domain.Models.Order>(new QueryDefinition("SELECT * FROM c"));
                List<Order.Domain.Models.Order> orders = new();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    orders.AddRange(response.ToList());
                }
                _logger.LogInformation("Retrieved {Count} orders from Cosmos DB.", orders.Count);
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders from Cosmos DB.");
                throw new InfrastructureException("Failed to retrieve all orders from Cosmos DB.", ex);
            }
        }

        public async Task AddOrderAsync(Order.Domain.Models.Order order)
        {
            _logger.LogInformation("Attempting to add order ID: {OrderId} to Cosmos DB.", order.id);
            try
            {
                await _container.CreateItemAsync(order, new PartitionKey(order.id));
                _logger.LogInformation("Order ID: {OrderId} added successfully to Cosmos DB.", order.id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order ID: {OrderId} to Cosmos DB.", order.id);
                throw new InfrastructureException($"Failed to add order ID: {order.id} to Cosmos DB.", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(string orderId, OrderStatus status)
        {
            _logger.LogInformation("Attempting to update status of order ID: {OrderId} to {Status} in Cosmos DB.", orderId, status);
            try
            {
                ItemResponse<Order.Domain.Models.Order> response = await _container.ReadItemAsync<Order.Domain.Models.Order>(orderId, new PartitionKey(orderId));
                var order = response.Resource;
                if (order == null)
                {
                    _logger.LogWarning("Order ID: {OrderId} not found for status update in Cosmos DB.", orderId);
                    throw new NotFoundException($"Order with ID: {orderId} not found for status update.");
                }
                order.Status = status;
                await _container.ReplaceItemAsync(order, orderId, new PartitionKey(orderId));
                _logger.LogInformation("Order ID: {OrderId} status updated to {Status} successfully in Cosmos DB.", orderId, status);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order ID: {OrderId} not found for status update in Cosmos DB.", orderId);
                throw new NotFoundException($"Order with ID: {orderId} not found for status update.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order ID: {OrderId} to {Status} in Cosmos DB.", orderId, status);
                throw new InfrastructureException($"Failed to update status for order ID: {orderId} in Cosmos DB.", ex);
            }
        }
    }
}
