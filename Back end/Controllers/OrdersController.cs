using Jumia.DTOs;
using Jumia.Jumia.Domain.Models;
using Jumia.Jumia.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jumia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 1. Get user's basket
            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null || !basket.Items.Any())
            {
                return BadRequest(new { Message = "Your shopping cart is empty" });
            }

            // 2. Get delivery method
            var deliveryMethod = await _context.DeliveryMethods.FindAsync(orderCreateDto.DeliveryMethodId);
            if (deliveryMethod == null)
            {
                return BadRequest(new { Message = "Invalid DeliveryMethodId" });
            }

            // Start Transaction to guarantee database consistency
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orderItems = new List<OrderItem>();
                decimal subtotal = 0;

                // 3. Process each item, check stock, freeze details
                foreach (var item in basket.Items)
                {
                    var product = item.Product;
                    if (product == null)
                    {
                        return BadRequest(new { Message = "One or more products in your cart no longer exist" });
                    }

                    if (product.Stock < item.Quantity)
                    {
                        return BadRequest(new { Message = $"Product '{product.Name}' is out of stock or insufficient stock. Available: {product.Stock}" });
                    }

                    // Subtract stock
                    product.Stock -= item.Quantity;

                    // Create OrderItem
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        PictureUrl = product.PictureUrl,
                        Price = product.Price,
                        Quantity = item.Quantity
                    };

                    orderItems.Add(orderItem);
                    subtotal += product.Price * item.Quantity;
                }

                // 4. Create Order entity
                var order = new Order
                {
                    UserId = userId,
                    DeliveryMethodId = deliveryMethod.Id,
                    DeliveryPrice = deliveryMethod.Price,
                    SubTotal = subtotal,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    OrderItems = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Saves order so we get its ID

                // 5. Add Shipping Address linked to order
                var shippingAddress = new ShippingAddress
                {
                    OrderId = order.Id,
                    FirstName = orderCreateDto.ShippingAddress.FirstName,
                    LastName = orderCreateDto.ShippingAddress.LastName,
                    Street = orderCreateDto.ShippingAddress.Street,
                    City = orderCreateDto.ShippingAddress.City,
                    State = orderCreateDto.ShippingAddress.State,
                    ZipCode = orderCreateDto.ShippingAddress.ZipCode
                };

                _context.ShippingAddresses.Add(shippingAddress);

                // 6. Clear basket items
                _context.BasketItems.RemoveRange(basket.Items);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load complete order details for returning
                var savedOrder = await _context.Orders
                    .Include(o => o.DeliveryMethod)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                return Ok(MapToOrderDto(savedOrder!));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred while placing the order", Details = ex.Message });
            }
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.DeliveryMethod)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderDtos = orders.Select(MapToOrderDto).ToList();
            return Ok(orderDtos);
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.DeliveryMethod)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { Message = $"Order with ID {id} not found" });
            }

            return Ok(MapToOrderDto(order));
        }

        // GET: api/orders/deliveryMethods
        [HttpGet("deliveryMethods")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DeliveryMethod>>> GetDeliveryMethods()
        {
            var methods = await _context.DeliveryMethods.ToListAsync();
            return Ok(methods);
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                SubTotal = order.SubTotal,
                DeliveryPrice = order.DeliveryPrice,
                DeliveryMethodName = order.DeliveryMethod != null ? order.DeliveryMethod.ShortName : string.Empty,
                ShippingAddress = order.ShippingAddress != null ? new ShippingAddressDto
                {
                    FirstName = order.ShippingAddress.FirstName,
                    LastName = order.ShippingAddress.LastName,
                    Street = order.ShippingAddress.Street,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    ZipCode = order.ShippingAddress.ZipCode
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    PictureUrl = oi.PictureUrl,
                    Price = oi.Price,
                    Quantity = oi.Quantity
                }).ToList(),
                Total = order.SubTotal + order.DeliveryPrice
            };
        }
    }
}
