namespace Order.Infrastructure.Settings
{
    public class EventHubSettings
    {
        public required string ConnectionString { get; set; }
        public required string EventHubName { get; set; }
        public required string ConsumerGroup { get; set; }
        public required string BlobStorageConnectionString { get; set; }
        public required string BlobContainerName { get; set; }
    }
}
