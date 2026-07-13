using Jumia.DTOs;
using Jumia.Jumia.Domain.Models;
using Jumia.Jumia.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jumia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BrandsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    PictureUrl = b.PictureUrl
                })
                .ToListAsync();

            return Ok(brands);
        }

        // GET: api/brands/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);

            if (brand == null)
            {
                return NotFound(new { Message = $"Brand with ID {id} not found" });
            }

            var brandDto = new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                PictureUrl = brand.PictureUrl
            };

            return Ok(brandDto);
        }

        // POST: api/brands
        [HttpPost]
        public async Task<ActionResult<BrandDto>> CreateBrand(BrandDto brandDto)
        {
            var brand = new Brand
            {
                Name = brandDto.Name,
                PictureUrl = brandDto.PictureUrl
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            brandDto.Id = brand.Id;

            return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, brandDto);
        }

        // PUT: api/brands/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, BrandDto brandDto)
        {
            if (id != brandDto.Id)
            {
                return BadRequest(new { Message = "ID mismatch" });
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound(new { Message = $"Brand with ID {id} not found" });
            }

            brand.Name = brandDto.Name;
            brand.PictureUrl = brandDto.PictureUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrandExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/brands/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound(new { Message = $"Brand with ID {id} not found" });
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Brand deleted successfully" });
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
