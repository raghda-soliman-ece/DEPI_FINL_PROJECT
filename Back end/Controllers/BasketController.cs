using Jumia.DTOs;
using Jumia.Jumia.Domain.Models;
using Jumia.Jumia.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jumia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/basket
        [HttpGet]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var basket = await GetOrCreateUserBasket(userId);

            return Ok(MapToBasketDto(basket));
        }

        // POST: api/basket/items
        [HttpPost("items")]
        public async Task<ActionResult<BasketDto>> AddOrUpdateItem([FromQuery] int productId, [FromQuery] int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (quantity <= 0)
            {
                return BadRequest(new { Message = "Quantity must be greater than 0" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {productId} not found" });
            }

            if (product.Stock < quantity)
            {
                return BadRequest(new { Message = $"Insufficient stock. Only {product.Stock} items available." });
            }

            var basket = await GetOrCreateUserBasket(userId);
            var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity; // Update quantity
            }
            else
            {
                basket.Items.Add(new BasketItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();

            // Reload basket with updated relations
            basket = await GetOrCreateUserBasket(userId);

            return Ok(MapToBasketDto(basket));
        }

        // DELETE: api/basket/items/5
        [HttpDelete("items/{productId}")]
        public async Task<ActionResult<BasketDto>> RemoveItem(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var basket = await GetOrCreateUserBasket(userId);
            var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                basket.Items.Remove(item);
                await _context.SaveChangesAsync();
            }

            // Reload basket with updated relations
            basket = await GetOrCreateUserBasket(userId);

            return Ok(MapToBasketDto(basket));
        }

        // DELETE: api/basket
        [HttpDelete]
        public async Task<IActionResult> ClearBasket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket != null)
            {
                _context.BasketItems.RemoveRange(basket.Items);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "Basket cleared successfully" });
        }

        private async Task<Basket> GetOrCreateUserBasket(string userId)
        {
            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null)
            {
                basket = new Basket { UserId = userId };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();

                // Fetch again to ensure references are loaded
                basket = await _context.Baskets
                    .Include(b => b.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(b => b.UserId == userId);
            }

            return basket!;
        }

        private BasketDto MapToBasketDto(Basket basket)
        {
            var itemsDto = basket.Items.Select(i => new BasketItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product != null ? i.Product.Name : string.Empty,
                ProductPictureUrl = i.Product != null ? i.Product.PictureUrl : string.Empty,
                ProductPrice = i.Product != null ? i.Product.Price : 0.00m,
                Quantity = i.Quantity
            }).ToList();

            return new BasketDto
            {
                Id = basket.Id,
                UserId = basket.UserId,
                Items = itemsDto,
                TotalPrice = itemsDto.Sum(i => i.ProductPrice * i.Quantity)
            };
        }
    }
}
