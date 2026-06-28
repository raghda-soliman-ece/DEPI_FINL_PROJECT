using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jumia.Jumia.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jumia.Jumia.Infrastructure
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                // 1. Seed Brands
                if (!await context.Brands.AnyAsync())
                {
                    var brands = new List<Brand>
                    {
                        new Brand { Name = "Apple", PictureUrl = "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?w=300" },
                        new Brand { Name = "Samsung", PictureUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=300" },
                        new Brand { Name = "Sony", PictureUrl = "https://images.unsplash.com/photo-1616440347437-b1c73416efc2?w=300" },
                        new Brand { Name = "Dell", PictureUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=300" },
                        new Brand { Name = "Adidas", PictureUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300" },
                        new Brand { Name = "Nike", PictureUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=300" }
                    };

                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                }

                // 2. Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    // Parent Categories
                    var electronics = new Category { Name = "Electronics", Description = "Phones, Laptops, Accessories and more", PictureUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=300" };
                    var fashion = new Category { Name = "Fashion", Description = "Clothing, shoes and apparel", PictureUrl = "https://images.unsplash.com/photo-1483985988355-763728e1935b?w=300" };
                    var home = new Category { Name = "Home & Kitchen", Description = "Appliances and home decoration", PictureUrl = "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?w=300" };

                    await context.Categories.AddRangeAsync(electronics, fashion, home);
                    await context.SaveChangesAsync();

                    // Sub categories
                    var phones = new Category { Name = "Smartphones", ParentCategoryId = electronics.Id, PictureUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=300" };
                    var laptops = new Category { Name = "Laptops", ParentCategoryId = electronics.Id, PictureUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=300" };
                    var shoes = new Category { Name = "Shoes", ParentCategoryId = fashion.Id, PictureUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300" };
                    var clothes = new Category { Name = "Clothing", ParentCategoryId = fashion.Id, PictureUrl = "https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=300" };

                    await context.Categories.AddRangeAsync(phones, laptops, shoes, clothes);
                    await context.SaveChangesAsync();
                }

                // 3. Seed Delivery Methods
                if (!await context.DeliveryMethods.AnyAsync())
                {
                    var deliveryMethods = new List<DeliveryMethod>
                    {
                        new DeliveryMethod { ShortName = "Free", Description = "Super saver shipping. Takes a bit longer", DeliveryTime = "7-10 Days", Price = 0.00m },
                        new DeliveryMethod { ShortName = "Standard", Description = "Delivery straight to your door step", DeliveryTime = "3-5 Days", Price = 15.00m },
                        new DeliveryMethod { ShortName = "Express", Description = "Next day delivery guaranteed", DeliveryTime = "1-2 Days", Price = 40.00m }
                    };

                    await context.DeliveryMethods.AddRangeAsync(deliveryMethods);
                    await context.SaveChangesAsync();
                }

                // 4. Seed Products
                if (!await context.Products.AnyAsync())
                {
                    var appleBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Apple");
                    var samsungBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Samsung");
                    var sonyBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Sony");
                    var dellBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Dell");
                    var adidasBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Adidas");
                    var nikeBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Nike");

                    var phoneCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Smartphones");
                    var laptopCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Laptops");
                    var shoeCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Shoes");

                    if (appleBrand != null && samsungBrand != null && sonyBrand != null && dellBrand != null &&
                        adidasBrand != null && nikeBrand != null && phoneCat != null && laptopCat != null && shoeCat != null)
                    {
                        var products = new List<Product>
                        {
                            new Product
                            {
                                Name = "Apple iPhone 15 Pro",
                                Description = "Experience the absolute best of Apple technology with the lightweight and durable aerospace-grade titanium frame, high-performance A17 Pro chip, custom Action button, and a powerful camera system.",
                                Price = 1199.99m,
                                OldPrice = 1299.99m,
                                PictureUrl = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=500",
                                Stock = 50,
                                CategoryId = phoneCat.Id,
                                BrandId = appleBrand.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-10),
                                IsActive = true
                            },
                            new Product
                            {
                                Name = "Samsung Galaxy S24 Ultra",
                                Description = "Welcome to the era of mobile AI. With Galaxy S24 Ultra in your hands, you can unleash whole new levels of creativity, productivity and possibility - starting with the most important device in your life. Your smartphone.",
                                Price = 1299.99m,
                                OldPrice = 1399.99m,
                                PictureUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500",
                                Stock = 35,
                                CategoryId = phoneCat.Id,
                                BrandId = samsungBrand.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-5),
                                IsActive = true
                            },
                            new Product
                            {
                                Name = "Dell XPS 15 Laptop",
                                Description = "The XPS 15 laptop delivers a powerful combination of 13th Gen Intel Core processors and NVIDIA GeForce RTX graphics to keep your creative projects flowing.",
                                Price = 1899.99m,
                                OldPrice = 2099.99m,
                                PictureUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500",
                                Stock = 15,
                                CategoryId = laptopCat.Id,
                                BrandId = dellBrand.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-12),
                                IsActive = true
                            },
                            new Product
                            {
                                Name = "Sony WH-1000XM5 Wireless Headphones",
                                Description = "Industry-leading noise cancellation, exceptional sound quality, crystal clear hands-free calling, and up to 30 hours of battery life with quick charging.",
                                Price = 349.99m,
                                OldPrice = 399.99m,
                                PictureUrl = "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?w=500",
                                Stock = 40,
                                CategoryId = phoneCat.ParentCategoryId ?? phoneCat.Id, // Fallback to Parent Category Electronics
                                BrandId = sonyBrand.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-3),
                                IsActive = true
                            },
                            new Product
                            {
                                Name = "Adidas Ultraboost Light Running Shoes",
                                Description = "Run like the wind with the lightest Ultraboost ever made. Featuring revolutionary Light BOOST cushioning, these shoes offer ultimate responsiveness and comfort for daily runs.",
                                Price = 179.99m,
                                OldPrice = 199.99m,
                                PictureUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500",
                                Stock = 100,
                                CategoryId = shoeCat.Id,
                                BrandId = adidasBrand.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-2),
                                IsActive = true
                            },
                            new Product
                            {
                                Name = "Nike Air Max 270",
                                Description = "Nike's first lifestyle Air Max brings you style, comfort and big attitude. Features a large Air unit wrapped around the heel for bouncy support, premium knit upper, and iconic sporty details.",
                                Price = 149.99m,
                                OldPrice = 169.99m,
                                PictureUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=500",
                                Stock = 80,
                                CategoryId = shoeCat.Id,
                                BrandId = nikeBrand.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-1),
                                IsActive = true
                            }
                        };

                        await context.Products.AddRangeAsync(products);
                        await context.SaveChangesAsync();

                        // 5. Add Product Images
                        foreach (var product in products)
                        {
                            var images = new List<ProductImage>
                            {
                                new ProductImage { ImageUrl = product.PictureUrl ?? string.Empty, IsMain = true, ProductId = product.Id },
                                new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=500", IsMain = false, ProductId = product.Id }
                            };
                            await context.ProductImages.AddRangeAsync(images);
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger("DbSeeder");
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
