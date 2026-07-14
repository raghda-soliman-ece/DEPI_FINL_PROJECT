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

                // 6. Seed the 36 products that used to only exist as static text in the front-end HTML pages
                // (mobiles/electronics/home-appliances/fashion/supermarket/kids), so they are searchable via the API.
                // This step runs every time and only inserts products that are missing by name, so it's safe to
                // deploy repeatedly without creating duplicates.
                await SeedFrontEndCatalogAsync(context);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger("DbSeeder");
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task<Brand> GetOrCreateBrandAsync(AppDbContext context, string name)
        {
            var brand = await context.Brands.FirstOrDefaultAsync(b => b.Name == name);
            if (brand == null)
            {
                brand = new Brand { Name = name, PictureUrl = string.Empty };
                context.Brands.Add(brand);
                await context.SaveChangesAsync();
            }
            return brand;
        }

        private static async Task<Category> GetOrCreateCategoryAsync(AppDbContext context, string name, int? parentCategoryId = null)
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == name);
            if (category == null)
            {
                category = new Category { Name = name, ParentCategoryId = parentCategoryId };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }
            return category;
        }

        private static async Task SeedFrontEndCatalogAsync(AppDbContext context)
        {
            // Get or create the categories these products belong to.
            var electronicsCat = await GetOrCreateCategoryAsync(context, "Electronics");
            var phonesCat = await GetOrCreateCategoryAsync(context, "Smartphones", electronicsCat.Id);
            var fashionCat = await GetOrCreateCategoryAsync(context, "Fashion");
            var homeKitchenCat = await GetOrCreateCategoryAsync(context, "Home & Kitchen");
            var homeAppliancesCat = await GetOrCreateCategoryAsync(context, "Home Appliances", homeKitchenCat.Id);
            var supermarketCat = await GetOrCreateCategoryAsync(context, "Supermarket");
            var kidsCat = await GetOrCreateCategoryAsync(context, "Kids");

            // Get or create the brands referenced below.
            var samsung = await GetOrCreateBrandAsync(context, "Samsung");
            var apple = await GetOrCreateBrandAsync(context, "Apple");
            var xiaomi = await GetOrCreateBrandAsync(context, "Xiaomi");
            var oppo = await GetOrCreateBrandAsync(context, "Oppo");
            var sony = await GetOrCreateBrandAsync(context, "Sony");
            var lenovo = await GetOrCreateBrandAsync(context, "Lenovo");
            var canon = await GetOrCreateBrandAsync(context, "Canon");
            var playstation = await GetOrCreateBrandAsync(context, "Sony PlayStation");
            var toshiba = await GetOrCreateBrandAsync(context, "Toshiba");
            var lg = await GetOrCreateBrandAsync(context, "LG");
            var philips = await GetOrCreateBrandAsync(context, "Philips");
            var safclon = await GetOrCreateBrandAsync(context, "Other");
            var dyson = await GetOrCreateBrandAsync(context, "Dyson");
            var adidas = await GetOrCreateBrandAsync(context, "Adidas");
            var nike = await GetOrCreateBrandAsync(context, "Nike");
            var levis = await GetOrCreateBrandAsync(context, "Levi's");
            var casio = await GetOrCreateBrandAsync(context, "Casio");
            var colavita = await GetOrCreateBrandAsync(context, "Colavita");
            var baraka = await GetOrCreateBrandAsync(context, "Baraka");
            var ferrero = await GetOrCreateBrandAsync(context, "Ferrero");
            var ariel = await GetOrCreateBrandAsync(context, "Ariel");
            var nescafe = await GetOrCreateBrandAsync(context, "Nescafe");
            var nestle = await GetOrCreateBrandAsync(context, "Nestle");
            var lego = await GetOrCreateBrandAsync(context, "Lego");
            var barbie = await GetOrCreateBrandAsync(context, "Barbie");

            var catalog = new List<Product>
            {
                new Product { Name = "سامسونج جالاكسي S24 - 256 جيجا", Price = 32999m, PictureUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = phonesCat.Id, BrandId = samsung.Id, IsActive = true },
                new Product { Name = "أبل آيفون 15 - 128 جيجا", Price = 45999m, PictureUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = phonesCat.Id, BrandId = apple.Id, IsActive = true },
                new Product { Name = "سامسونج جالاكسي A54 - 128 جيجا", Price = 15499m, PictureUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = phonesCat.Id, BrandId = samsung.Id, IsActive = true },
                new Product { Name = "شاومي 14 Pro - 512 جيجا", Price = 28500m, PictureUrl = "images/redmi.jpg", Stock = 50, CategoryId = phonesCat.Id, BrandId = xiaomi.Id, IsActive = true },
                new Product { Name = "أوبو A78 - 256 جيجا، 8 رام", Price = 10999m, PictureUrl = "https://images.unsplash.com/photo-1585060544812-6b45742d762f?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = phonesCat.Id, BrandId = oppo.Id, IsActive = true },
                new Product { Name = "سامسونج جالاكسي تاب A8 - 64 جيجا", Price = 11999m, PictureUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = phonesCat.Id, BrandId = samsung.Id, IsActive = true },

                new Product { Name = "تلفزيون سامسونج QLED 55 بوصة 4K", Price = 28999m, PictureUrl = "https://images.unsplash.com/photo-1593784991095-a205069470b6?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = electronicsCat.Id, BrandId = samsung.Id, IsActive = true },
                new Product { Name = "سماعات Apple AirPods Pro الجيل الثاني", Price = 8999m, PictureUrl = "images/airpods.jpg", Stock = 50, CategoryId = electronicsCat.Id, BrandId = apple.Id, IsActive = true },
                new Product { Name = "لاب توب لينوفو ايديا باد 3 - Intel i5", Price = 22999m, PictureUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = electronicsCat.Id, BrandId = lenovo.Id, IsActive = true },
                new Product { Name = "كاميرا Canon EOS R50 - 24 ميجابكسل", Price = 19500m, PictureUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = electronicsCat.Id, BrandId = canon.Id, IsActive = true },
                new Product { Name = "PlayStation 5 - 1TB SSD", Price = 24999m, PictureUrl = "images/ps5.jpg", Stock = 50, CategoryId = electronicsCat.Id, BrandId = playstation.Id, IsActive = true },
                new Product { Name = "سماعة Sony WH-1000XM5 - ضد الضوضاء", Price = 12999m, PictureUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = electronicsCat.Id, BrandId = sony.Id, IsActive = true },

                new Product { Name = "ثلاجة توشيبا 20 قدم نوفروست - ستانلس", Price = 22500m, PictureUrl = "https://images.unsplash.com/photo-1584568694244-14fbdf83bd30?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = homeAppliancesCat.Id, BrandId = toshiba.Id, IsActive = true },
                new Product { Name = "غسالة LG ذات حوضين 12 كيلو - أبيض", Price = 14999m, PictureUrl = "https://images.unsplash.com/photo-1626806787461-102c1bfaaea1?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = homeAppliancesCat.Id, BrandId = lg.Id, IsActive = true },
                new Product { Name = "مكيف يونيون اير سبليت 1.5 حصان بارد", Price = 13500m, PictureUrl = "images/ac.jpg", Stock = 50, CategoryId = homeAppliancesCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "قلاية هوائية فيليبس XL - 6.2 لتر", Price = 6499m, PictureUrl = "images/airfryer.jpg", Stock = 50, CategoryId = homeAppliancesCat.Id, BrandId = philips.Id, IsActive = true },
                new Product { Name = "طقم حلل جرانيت سافلون 9 قطع", Price = 4200m, PictureUrl = "images/pots.jpg", Stock = 50, CategoryId = homeAppliancesCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "مكنسة كهربائية Dyson V12 - لاسلكية", Price = 18999m, PictureUrl = "images/vacuum.jpg", Stock = 50, CategoryId = homeAppliancesCat.Id, BrandId = dyson.Id, IsActive = true },

                new Product { Name = "حذاء ركض Adidas Ultraboost - أسود", Price = 5200m, PictureUrl = "images/shoes.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = adidas.Id, IsActive = true },
                new Product { Name = "تيشيرت رجالي Nike قطن 100% - أبيض", Price = 599m, PictureUrl = "images/tshirt.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = nike.Id, IsActive = true },
                new Product { Name = "شنطة يد حريمي جلد طبيعي - بني", Price = 2999m, PictureUrl = "images/bag.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "بنطلون جينز Levi's 501 - أزرق", Price = 1899m, PictureUrl = "images/jeans.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = levis.Id, IsActive = true },
                new Product { Name = "فستان سهرة حريمي - أحمر فاقع", Price = 1499m, PictureUrl = "images/dress.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "ساعة Casio G-Shock رجالي - أسود", Price = 3200m, PictureUrl = "images/watch.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = casio.Id, IsActive = true },

                new Product { Name = "زيت زيتون Colavita خالص 750 مل", Price = 189m, PictureUrl = "images/oil.jpg", Stock = 50, CategoryId = supermarketCat.Id, BrandId = colavita.Id, IsActive = true },
                new Product { Name = "لبن Baraka كامل الدسم 1 لتر × 6", Price = 155m, PictureUrl = "images/baraka.jpg", Stock = 50, CategoryId = supermarketCat.Id, BrandId = baraka.Id, IsActive = true },
                new Product { Name = "شوكولاتة Ferrero Rocher 30 حبة", Price = 299m, PictureUrl = "images/ferrero.jpg", Stock = 50, CategoryId = supermarketCat.Id, BrandId = ferrero.Id, IsActive = true },
                new Product { Name = "مسحوق غسيل Ariel 5 كيلو", Price = 279m, PictureUrl = "images/ariel.jpg", Stock = 50, CategoryId = supermarketCat.Id, BrandId = ariel.Id, IsActive = true },
                new Product { Name = "قهوة Nescafé Classic 200 جرام", Price = 149m, PictureUrl = "images/nescafe.jpg", Stock = 50, CategoryId = supermarketCat.Id, BrandId = nescafe.Id, IsActive = true },
                new Product { Name = "مياه Nestle Pure Life 1.5 لتر × 12", Price = 89m, PictureUrl = "images/water.jpg", Stock = 50, CategoryId = supermarketCat.Id, BrandId = nestle.Id, IsActive = true },

                new Product { Name = "مجموعة LEGO City 300 قطعة - سيارات", Price = 1299m, PictureUrl = "images/lego.jpg", Stock = 50, CategoryId = kidsCat.Id, BrandId = lego.Id, IsActive = true },
                new Product { Name = "عروسة Barbie Fashionista مع ملحقات", Price = 599m, PictureUrl = "images/barbie.jpg", Stock = 50, CategoryId = kidsCat.Id, BrandId = barbie.Id, IsActive = true },
                new Product { Name = "روبوت ذكاء اصطناعي تعليمي للأطفال", Price = 2199m, PictureUrl = "images/robot.jpg", Stock = 50, CategoryId = kidsCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "دراجة هوائية للأطفال 16 بوصة", Price = 1499m, PictureUrl = "images/bike.jpg", Stock = 50, CategoryId = kidsCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "طقم رسم وتلوين احترافي للأطفال 48 قلم", Price = 399m, PictureUrl = "images/colors.jpg", Stock = 50, CategoryId = kidsCat.Id, BrandId = safclon.Id, IsActive = true },
                new Product { Name = "حوض سباحة منزلي للأطفال 300×180 سم", Price = 899m, PictureUrl = "images/pool.jpg", Stock = 50, CategoryId = kidsCat.Id, BrandId = safclon.Id, IsActive = true }
            };

            var existingNames = await context.Products
                .Where(p => catalog.Select(c => c.Name).Contains(p.Name))
                .Select(p => p.Name)
                .ToListAsync();

            var toAdd = catalog.Where(p => !existingNames.Contains(p.Name)).ToList();
            if (!toAdd.Any()) return;

            foreach (var product in toAdd)
            {
                product.CreatedAt = DateTime.UtcNow;
            }

            await context.Products.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();

            foreach (var product in toAdd)
            {
                context.ProductImages.Add(new ProductImage
                {
                    ImageUrl = product.PictureUrl ?? string.Empty,
                    IsMain = true,
                    ProductId = product.Id
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
