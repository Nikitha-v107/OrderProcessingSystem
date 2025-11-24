using Microsoft.AspNetCore.Mvc;
using Order.Domain.DTOs;
using Order.Domain.Models;
using Order.Infrastructure.EventGrid;
using Order.Infrastructure.Repositories;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;

        public OrdersController(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            var order = new Order.Domain.Models.Order
            {
                Id = Guid.NewGuid().ToString(),
                CustomerName = createOrderDto.CustomerName,
                ProductName = createOrderDto.ProductName,
                Quantity = createOrderDto.Quantity,
                TotalAmount = createOrderDto.TotalAmount,
                Status = "Created",
                CreatedAtUtc = DateTime.UtcNow
            };

            await _orderRepository.AddOrderAsync(order);

            var orderCreatedDto = new OrderCreatedDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAtUtc = order.CreatedAtUtc
            };

            await _eventPublisher.PublishOrderCreatedEventAsync(orderCreatedDto);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(string id)
        {
            var order = await _orderRepository.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return Ok(orders);
        }
    }
}
