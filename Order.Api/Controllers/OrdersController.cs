using Microsoft.AspNetCore.Mvc;
using Order.Domain.DTOs;
using Order.Domain.Models;
using Order.Infrastructure.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Order.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // Apply Authorize attribute to the entire controller
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, IMapper mapper, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid CreateOrderDto received: {@ModelErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(createOrderDto);
                var orderDto = _mapper.Map<OrderDto>(order);
                _logger.LogInformation("Order created successfully with ID: {OrderId}", order.id);
                return CreatedAtAction(nameof(GetOrder), new { orderId = order.id }, orderDto);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating order: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InfrastructureException ex)
            {
                _logger.LogError(ex, "Infrastructure error creating order.");
                return Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred while creating the order.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while creating order.");
                return Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred.");
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("Invalid ID received for GetOrder.");
                return BadRequest("Order ID cannot be null or empty.");
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                var orderDto = _mapper.Map<OrderDto>(order);
                _logger.LogInformation("Order with ID: {OrderId} retrieved successfully.", orderId);
                return Ok(orderDto);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error retrieving order with ID: {OrderId}: {Message}", orderId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                _logger.LogInformation(ex, "Order with ID: {OrderId} not found.", orderId);
                return NotFound(ex.Message);
            }
            catch (InfrastructureException ex)
            {
                _logger.LogError(ex, "Infrastructure error retrieving order with ID: {OrderId}.", orderId);
                return Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred while retrieving the order.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while retrieving order with ID: {OrderId}.", orderId);
                return Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                var orderDtos = _mapper.Map<List<OrderDto>>(orders);
                _logger.LogInformation("Retrieved all orders successfully.");
                return Ok(orderDtos);
            }
            catch (InfrastructureException ex)
            {
                _logger.LogError(ex, "Infrastructure error retrieving all orders.");
                return Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred while retrieving orders.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while retrieving all orders.");
                return Problem(statusCode: 500, title: "Internal Server Error", detail: "An unexpected error occurred.");
            }
        }
    }
}
