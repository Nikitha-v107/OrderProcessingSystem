using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Order.Domain.Models;
using Order.Infrastructure.Settings;

namespace Order.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly Container _container;

        public OrderRepository(CosmosClient cosmosClient, IOptions<CosmosDbSettings> settings)
        {
            _container = cosmosClient.GetDatabase(settings.Value.DatabaseName).GetContainer(settings.Value.ContainerName);
        }

        public async Task<Order.Domain.Models.Order> GetOrderAsync(string id)
        {
            try
            {
                ItemResponse<Order.Domain.Models.Order> response = await _container.ReadItemAsync<Order.Domain.Models.Order>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Order.Domain.Models.Order>> GetOrdersAsync()
        {
            var query = _container.GetItemQueryIterator<Order.Domain.Models.Order>(new QueryDefinition("SELECT * FROM c"));
            List<Order.Domain.Models.Order> orders = new();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                orders.AddRange(response.ToList());
            }
            return orders;
        }

        public async Task AddOrderAsync(Order.Domain.Models.Order order)
        {
            await _container.CreateItemAsync(order, new PartitionKey(order.Id));
        }

        public async Task UpdateOrderStatusAsync(string id, string status)
        {
            ItemResponse<Order.Domain.Models.Order> response = await _container.ReadItemAsync<Order.Domain.Models.Order>(id, new PartitionKey(id));
            var order = response.Resource;
            order.Status = status;
            await _container.ReplaceItemAsync(order, id, new PartitionKey(id));
        }
    }
}
