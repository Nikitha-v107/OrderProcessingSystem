using AutoMapper;
using Microsoft.Extensions.Logging;
using Order.Domain.DTOs;
using Order.Domain.Enums;
using Order.Domain.Models;
using Order.Infrastructure.EventGrid;
using Order.Infrastructure.Repositories;
using Order.Domain.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Order.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher, IMapper mapper, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Order.Domain.Models.Order> CreateOrderAsync(CreateOrderDto dto)
        {
            _logger.LogInformation("Attempting to create a new order for customer: {CustomerName}", dto.CustomerName);

            if (dto == null)
            {
                throw new ValidationException("Order data cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
            {
                throw new ValidationException("Customer name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(dto.ProductName))
            {
                throw new ValidationException("Product name cannot be empty.");
            }
            if (dto.Quantity <= 0)
            {
                throw new ValidationException("Quantity must be greater than 0.");
            }
            if (dto.TotalAmount <= 0)
            {
                throw new ValidationException("Total amount must be greater than 0.");
            }

            try
            {
                var order = _mapper.Map<Order.Domain.Models.Order>(dto);
                order.id = Guid.NewGuid().ToString(); // Ensure lowercase id
                order.Status = OrderStatus.Created;
                order.CreatedAtUtc = DateTime.UtcNow;

                await _orderRepository.AddOrderAsync(order);
                _logger.LogInformation("Order ID: {OrderId} created in Cosmos DB.", order.id);

                var orderCreatedDto = _mapper.Map<OrderCreatedDto>(order);
                await _eventPublisher.PublishOrderCreatedEventAsync(orderCreatedDto);
                _logger.LogInformation("Order.Created event published for Order ID: {OrderId}", order.id);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for customer: {CustomerName}", dto.CustomerName);
                throw new InfrastructureException("Failed to create order due to an infrastructure issue.", ex);
            }
        }

        public async Task<Order.Domain.Models.Order?> GetOrderByIdAsync(string orderId)
        {
            _logger.LogInformation("Attempting to retrieve order by ID: {OrderId}", orderId);
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new ValidationException("Order ID cannot be null or empty.");
            }

            try
            {
                var order = await _orderRepository.GetOrderAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order with ID: {OrderId} not found.", orderId);
                    throw new NotFoundException($"Order with ID: {orderId} not found.");
                }
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID: {OrderId}.", orderId);
                throw new InfrastructureException($"Failed to retrieve order with ID: {orderId} due to an infrastructure issue.", ex);
            }
        }

        public async Task<List<Order.Domain.Models.Order>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Attempting to retrieve all orders.");
            try
            {
                var orders = (await _orderRepository.GetOrdersAsync()).ToList();
                _logger.LogInformation("Retrieved {Count} orders.", orders.Count);
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders.");
                throw new InfrastructureException("Failed to retrieve all orders due to an infrastructure issue.", ex);
            }
        }
    }
}
