using Microsoft.Azure.Cosmos;
using Order.Worker;
using Order.Infrastructure.Repositories;
using Order.Infrastructure.Settings;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<CosmosDbSettings>(hostContext.Configuration.GetSection("CosmosDb"));
        services.Configure<EventHubSettings>(hostContext.Configuration.GetSection("EventHub"));

        services.AddSingleton((provider) =>
        {
            var cosmosDbSettings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbSettings>>().Value;
            return new CosmosClient(cosmosDbSettings.ConnectionString);
        });

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
