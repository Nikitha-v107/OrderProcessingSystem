using Microsoft.Azure.Cosmos;
using Order.Infrastructure.EventGrid;
using Order.Infrastructure.Repositories;
using Order.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));
builder.Services.Configure<EventGridSettings>(builder.Configuration.GetSection("EventGrid"));

builder.Services.AddSingleton((provider) =>
{
    var cosmosDbSettings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbSettings>>().Value;
    return new CosmosClient(cosmosDbSettings.ConnectionString);
});

builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
