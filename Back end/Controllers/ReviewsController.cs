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
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return NotFound(new { Message = $"Product with ID {productId} not found" });
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    UserId = r.UserId,
                    UserDisplayName = r.User != null ? r.User.DisplayName : "Anonymous",
                    ProductId = r.ProductId
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] ReviewCreateDto reviewCreateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var productExists = await _context.Products.AnyAsync(p => p.Id == reviewCreateDto.ProductId);
            if (!productExists)
            {
                return NotFound(new { Message = $"Product with ID {reviewCreateDto.ProductId} not found" });
            }

            // Check if user already reviewed this product
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == reviewCreateDto.ProductId);

            if (alreadyReviewed)
            {
                return BadRequest(new { Message = "You have already reviewed this product" });
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = reviewCreateDto.ProductId,
                Rating = reviewCreateDto.Rating,
                Comment = reviewCreateDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Load saved review user details
            var savedReview = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            var reviewDto = new ReviewDto
            {
                Id = savedReview!.Id,
                Comment = savedReview.Comment,
                Rating = savedReview.Rating,
                CreatedAt = savedReview.CreatedAt,
                UserId = savedReview.UserId,
                UserDisplayName = savedReview.User != null ? savedReview.User.DisplayName : "Anonymous",
                ProductId = savedReview.ProductId
            };

            return Ok(reviewDto);
        }
    }
}
