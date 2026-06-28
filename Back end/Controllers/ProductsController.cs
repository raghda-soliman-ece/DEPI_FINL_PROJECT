using Jumia.DTOs;
using Jumia.Jumia.Domain.Models;
using Jumia.Jumia.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jumia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] string? sort,
            [FromQuery] string? search,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 6)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // 1. Filtering
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // 2. Search
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower) || 
                                         (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            // 3. Sorting
            query = sort switch
            {
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default sorting
            };

            // 4. Pagination Count
            var totalItems = await query.CountAsync();

            // 5. Pagination Data
            var products = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OldPrice = p.OldPrice,
                PictureUrl = p.PictureUrl,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : string.Empty,
                CreatedAt = p.CreatedAt,
                IsActive = p.IsActive,
                Images = p.Images.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    IsMain = img.IsMain
                }).ToList()
            }).ToList();

            return Ok(new
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Count = totalItems,
                Data = productDtos
            });
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found" });
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                PictureUrl = product.PictureUrl,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null ? product.Category.Name : string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand != null ? product.Brand.Name : string.Empty,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive,
                Images = product.Images.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    IsMain = img.IsMain
                }).ToList(),
                Reviews = product.Reviews.Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    UserId = r.UserId,
                    UserDisplayName = r.User != null ? r.User.DisplayName : "Anonymous",
                    ProductId = r.ProductId
                }).ToList()
            };

            return Ok(productDto);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductCreateDto createDto)
        {
            // Verify brand and category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createDto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { Message = "Invalid CategoryId" });
            }

            var brandExists = await _context.Brands.AnyAsync(b => b.Id == createDto.BrandId);
            if (!brandExists)
            {
                return BadRequest(new { Message = "Invalid BrandId" });
            }

            var product = new Product
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                OldPrice = createDto.OldPrice,
                PictureUrl = createDto.PictureUrl,
                Stock = createDto.Stock,
                CategoryId = createDto.CategoryId,
                BrandId = createDto.BrandId,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Add initial image if picture url is provided
            if (!string.IsNullOrEmpty(product.PictureUrl))
            {
                var mainImage = new ProductImage
                {
                    ImageUrl = product.PictureUrl,
                    IsMain = true,
                    ProductId = product.Id
                };
                _context.ProductImages.Add(mainImage);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new { Message = "Product created successfully", ProductId = product.Id });
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto updateDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found" });
            }

            // Verify brand and category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateDto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { Message = "Invalid CategoryId" });
            }

            var brandExists = await _context.Brands.AnyAsync(b => b.Id == updateDto.BrandId);
            if (!brandExists)
            {
                return BadRequest(new { Message = "Invalid BrandId" });
            }

            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.Price = updateDto.Price;
            product.OldPrice = updateDto.OldPrice;
            product.PictureUrl = updateDto.PictureUrl;
            product.Stock = updateDto.Stock;
            product.CategoryId = updateDto.CategoryId;
            product.BrandId = updateDto.BrandId;
            product.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Product updated successfully" });
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Message = $"Product with ID {id} not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Product deleted successfully" });
        }
    }
}
