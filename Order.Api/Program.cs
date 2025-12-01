using Microsoft.Azure.Cosmos;
using Order.Infrastructure.EventGrid;
using Order.Infrastructure.Repositories;
using Order.Infrastructure.Settings;
using Order.Infrastructure.Services;
using System.Reflection;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (ensure it's still present if needed, or remove if the intent is to simplify as per the new Program.cs snippet)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog(); // Use Serilog for hosting logs

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ALWAYS enable Swagger (Production + Development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
    c.RoutePrefix = "swagger"; 
});

app.UseHttpsRedirection();

// Ensure static files are used before authorization
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

Log.CloseAndFlush();
