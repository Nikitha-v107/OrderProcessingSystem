namespace Order.Infrastructure.Settings
{
    public class CosmosDbSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string ContainerName { get; set; }
    }
}
