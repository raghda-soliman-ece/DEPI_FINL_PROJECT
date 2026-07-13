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
                        new Brand { Name = "Xiaomi", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Xiaomi" },
                        new Brand { Name = "Oppo", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Oppo" },
                        new Brand { Name = "Lenovo", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Lenovo" },
                        new Brand { Name = "Canon", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Canon" },
                        new Brand { Name = "Adidas", PictureUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300" },
                        new Brand { Name = "Nike", PictureUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=300" },
                        new Brand { Name = "Levi's", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Levis" },
                        new Brand { Name = "Casio", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Casio" },
                        new Brand { Name = "Toshiba", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Toshiba" },
                        new Brand { Name = "LG", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=LG" },
                        new Brand { Name = "Unionaire", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Unionaire" },
                        new Brand { Name = "Philips", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Philips" },
                        new Brand { Name = "Dyson", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Dyson" },
                        new Brand { Name = "Lego", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Lego" },
                        new Brand { Name = "Barbie", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Barbie" },
                        new Brand { Name = "Other", PictureUrl = "https://placehold.co/300x300/f5f5f5/999?text=Other" }
                    };

                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                }

                // 2. Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    var electronics = new Category { Name = "إلكترونيات", Description = "تلفزيونات، أجهزة منزلية والمزيد", PictureUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=300" };
                    var fashion = new Category { Name = "أزياء", Description = "ملابس وأحذية", PictureUrl = "https://images.unsplash.com/photo-1483985988355-763728e1935b?w=300" };
                    var home = new Category { Name = "أجهزة منزلية", Description = "أجهزة منزلية وتكييفات", PictureUrl = "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?w=300" };
                    var supermarket = new Category { Name = "سوبر ماركت", Description = "مواد غذائية", PictureUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=300" };
                    var kids = new Category { Name = "ألعاب أطفال", Description = "ألعاب ومستلزمات أطفال", PictureUrl = "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=300" };

                    await context.Categories.AddRangeAsync(electronics, fashion, home, supermarket, kids);
                    await context.SaveChangesAsync();

                    var phones = new Category { Name = "موبايلات", ParentCategoryId = electronics.Id, PictureUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=300" };
                    
                    await context.Categories.AddRangeAsync(phones);
                    await context.SaveChangesAsync();
                }

                // 3. Seed Delivery Methods
                if (!await context.DeliveryMethods.AnyAsync())
                {
                    var deliveryMethods = new List<DeliveryMethod>
                    {
                        new DeliveryMethod { ShortName = "مجاني", Description = "توصيل عادي", DeliveryTime = "7-10 أيام", Price = 0.00m },
                        new DeliveryMethod { ShortName = "عادي", Description = "توصيل سريع", DeliveryTime = "3-5 أيام", Price = 15.00m },
                        new DeliveryMethod { ShortName = "جوميا إكسبريس", Description = "توصيل في اليوم التالي", DeliveryTime = "1-2 أيام", Price = 40.00m }
                    };

                    await context.DeliveryMethods.AddRangeAsync(deliveryMethods);
                    await context.SaveChangesAsync();
                }

                // 4. Seed Products
                if (!await context.Products.AnyAsync())
                {
                    var appleBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Apple") ?? context.Brands.First();
                    var samsungBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Samsung") ?? context.Brands.First();
                    var sonyBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Sony") ?? context.Brands.First();
                    var xiaomiBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Xiaomi") ?? context.Brands.First();
                    var oppoBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Oppo") ?? context.Brands.First();
                    var lenovoBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Lenovo") ?? context.Brands.First();
                    var canonBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Canon") ?? context.Brands.First();
                    var adidasBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Adidas") ?? context.Brands.First();
                    var nikeBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Nike") ?? context.Brands.First();
                    var levisBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Levi's") ?? context.Brands.First();
                    var casioBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Casio") ?? context.Brands.First();
                    var toshibaBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Toshiba") ?? context.Brands.First();
                    var lgBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "LG") ?? context.Brands.First();
                    var unionaireBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Unionaire") ?? context.Brands.First();
                    var philipsBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Philips") ?? context.Brands.First();
                    var dysonBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Dyson") ?? context.Brands.First();
                    var legoBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Lego") ?? context.Brands.First();
                    var barbieBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Barbie") ?? context.Brands.First();
                    var otherBrand = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Other") ?? context.Brands.First();

                    var phoneCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "موبايلات") ?? context.Categories.First();
                    var elecCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "إلكترونيات") ?? context.Categories.First();
                    var fashionCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "أزياء") ?? context.Categories.First();
                    var homeCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "أجهزة منزلية") ?? context.Categories.First();
                    var superCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "سوبر ماركت") ?? context.Categories.First();
                    var kidsCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "ألعاب أطفال") ?? context.Categories.First();

                    var products = new List<Product>
                    {
                        // Mobiles
                        new Product { Name = "سامسونج جالاكسي S24 - 256 جيجا", Description = "سامسونج جالاكسي S24 - 256 جيجا", Price = 32999m, OldPrice = 35000m, PictureUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=300&q=80", Stock = 50, CategoryId = phoneCat.Id, BrandId = samsungBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "أبل آيفون 15 - 128 جيجا", Description = "أبل آيفون 15 - 128 جيجا", Price = 45999m, OldPrice = 48000m, PictureUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=300&q=80", Stock = 30, CategoryId = phoneCat.Id, BrandId = appleBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "سامسونج جالاكسي A54 - 128 جيجا", Description = "سامسونج جالاكسي A54 - 128 جيجا", Price = 15499m, OldPrice = 16999m, PictureUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?auto=format&fit=crop&w=300&q=80", Stock = 20, CategoryId = phoneCat.Id, BrandId = samsungBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "شاومي 14 Pro - 512 جيجا", Description = "شاومي 14 Pro - 512 جيجا", Price = 28500m, OldPrice = 30000m, PictureUrl = "images/redmi.jpg", Stock = 15, CategoryId = phoneCat.Id, BrandId = xiaomiBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "أوبو A78 - 256 جيجا، 8 رام", Description = "أوبو A78 - 256 جيجا، 8 رام", Price = 10999m, OldPrice = 12000m, PictureUrl = "https://images.unsplash.com/photo-1585060544812-6b45742d762f?auto=format&fit=crop&w=300&q=80", Stock = 40, CategoryId = phoneCat.Id, BrandId = oppoBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "سامسونج جالاكسي تاب A8 - 64 جيجا", Description = "سامسونج جالاكسي تاب A8 - 64 جيجا", Price = 11999m, OldPrice = 13500m, PictureUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=300&q=80", Stock = 25, CategoryId = phoneCat.Id, BrandId = samsungBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },

                        // Electronics
                        new Product { Name = "تلفزيون سامسونج QLED 55 بوصة 4K", Description = "تلفزيون سامسونج QLED 55 بوصة 4K", Price = 28999m, OldPrice = 32000m, PictureUrl = "https://images.unsplash.com/photo-1593784991095-a205069470b6?auto=format&fit=crop&w=300&q=80", Stock = 10, CategoryId = elecCat.Id, BrandId = samsungBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "سماعات Apple AirPods Pro الجيل الثاني", Description = "سماعات Apple AirPods Pro الجيل الثاني", Price = 8999m, OldPrice = 9500m, PictureUrl = "images/airpods.jpg", Stock = 100, CategoryId = elecCat.Id, BrandId = appleBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "لاب توب لينوفو ايديا باد 3 - Intel i5", Description = "لاب توب لينوفو ايديا باد 3 - Intel i5", Price = 22999m, OldPrice = 24500m, PictureUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=300&q=80", Stock = 12, CategoryId = elecCat.Id, BrandId = lenovoBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "كاميرا Canon EOS R50 - 24 ميجابكسل", Description = "كاميرا Canon EOS R50 - 24 ميجابكسل", Price = 19500m, OldPrice = 23000m, PictureUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=300&q=80", Stock = 8, CategoryId = elecCat.Id, BrandId = canonBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "PlayStation 5 - 1TB SSD", Description = "PlayStation 5 - 1TB SSD", Price = 24999m, OldPrice = 26000m, PictureUrl = "images/ps5.jpg", Stock = 30, CategoryId = elecCat.Id, BrandId = sonyBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "سماعة Sony WH-1000XM5 - ضد الضوضاء", Description = "سماعة Sony WH-1000XM5 - ضد الضوضاء", Price = 12999m, OldPrice = 14000m, PictureUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=300&q=80", Stock = 20, CategoryId = elecCat.Id, BrandId = sonyBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },

                        // Fashion
                        new Product { Name = "حذاء ركض Adidas Ultraboost - أسود", Description = "حذاء ركض Adidas Ultraboost - أسود", Price = 5200m, OldPrice = 6000m, PictureUrl = "images/shoes.jpg", Stock = 50, CategoryId = fashionCat.Id, BrandId = adidasBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "تيشيرت رجالي Nike قطن 100% - أبيض", Description = "تيشيرت رجالي Nike قطن 100% - أبيض", Price = 599m, OldPrice = 749m, PictureUrl = "images/tshirt.jpg", Stock = 150, CategoryId = fashionCat.Id, BrandId = nikeBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "شنطة يد حريمي جلد طبيعي - بني", Description = "شنطة يد حريمي جلد طبيعي - بني", Price = 2999m, OldPrice = 3500m, PictureUrl = "images/bag.jpg", Stock = 25, CategoryId = fashionCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "بنطلون جينز Levi's 501 - أزرق", Description = "بنطلون جينز Levi's 501 - أزرق", Price = 1899m, OldPrice = 2200m, PictureUrl = "images/jeans.jpg", Stock = 60, CategoryId = fashionCat.Id, BrandId = levisBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "فستان سهرة حريمي - أحمر فاقع", Description = "فستان سهرة حريمي - أحمر فاقع", Price = 1499m, OldPrice = 1800m, PictureUrl = "images/dress.jpg", Stock = 15, CategoryId = fashionCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "ساعة Casio G-Shock رجالي - أسود", Description = "ساعة Casio G-Shock رجالي - أسود", Price = 3200m, OldPrice = 3800m, PictureUrl = "images/watch.jpg", Stock = 45, CategoryId = fashionCat.Id, BrandId = casioBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },

                        // Home Appliances
                        new Product { Name = "ثلاجة توشيبا 20 قدم نوفروست - ستانلس", Description = "ثلاجة توشيبا 20 قدم نوفروست - ستانلس", Price = 22500m, OldPrice = 25000m, PictureUrl = "https://images.unsplash.com/photo-1584568694244-14fbdf83bd30?auto=format&fit=crop&w=300&q=80", Stock = 10, CategoryId = homeCat.Id, BrandId = toshibaBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "غسالة LG ذات حوضين 12 كيلو - أبيض", Description = "غسالة LG ذات حوضين 12 كيلو - أبيض", Price = 14999m, OldPrice = 16500m, PictureUrl = "https://images.unsplash.com/photo-1626806787461-102c1bfaaea1?auto=format&fit=crop&w=300&q=80", Stock = 15, CategoryId = homeCat.Id, BrandId = lgBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "مكيف يونيون اير سبليت 1.5 حصان بارد", Description = "مكيف يونيون اير سبليت 1.5 حصان بارد", Price = 13500m, OldPrice = 16000m, PictureUrl = "images/ac.jpg", Stock = 20, CategoryId = homeCat.Id, BrandId = unionaireBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "قلاية هوائية فيليبس XL - 6.2 لتر", Description = "قلاية هوائية فيليبس XL - 6.2 لتر", Price = 6499m, OldPrice = 7200m, PictureUrl = "images/airfryer.jpg", Stock = 35, CategoryId = homeCat.Id, BrandId = philipsBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "طقم حلل جرانيت سافلون 9 قطع", Description = "طقم حلل جرانيت سافلون 9 قطع", Price = 4200m, OldPrice = 5100m, PictureUrl = "images/pots.jpg", Stock = 50, CategoryId = homeCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "مكنسة كهربائية Dyson V12 - لاسلكية", Description = "مكنسة كهربائية Dyson V12 - لاسلكية", Price = 18999m, OldPrice = 20000m, PictureUrl = "images/vacuum.jpg", Stock = 12, CategoryId = homeCat.Id, BrandId = dysonBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },

                        // Supermarket
                        new Product { Name = "زيت زيتون Colavita خالص 750 مل", Description = "زيت زيتون Colavita خالص 750 مل", Price = 189m, OldPrice = 220m, PictureUrl = "images/oil.jpg", Stock = 200, CategoryId = superCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "لبن Baraka كامل الدسم 1 لتر × 6", Description = "لبن Baraka كامل الدسم 1 لتر × 6", Price = 155m, OldPrice = 175m, PictureUrl = "images/baraka.jpg", Stock = 300, CategoryId = superCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "شوكولاتة Ferrero Rocher 30 حبة", Description = "شوكولاتة Ferrero Rocher 30 حبة", Price = 299m, OldPrice = 350m, PictureUrl = "images/ferrero.jpg", Stock = 150, CategoryId = superCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "مسحوق غسيل Ariel 5 كيلو", Description = "مسحوق غسيل Ariel 5 كيلو", Price = 279m, OldPrice = 320m, PictureUrl = "images/ariel.jpg", Stock = 80, CategoryId = superCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },

                        // Kids
                        new Product { Name = "مجموعة LEGO City 300 قطعة - سيارات", Description = "مجموعة LEGO City 300 قطعة - سيارات", Price = 1299m, OldPrice = 1500m, PictureUrl = "images/lego.jpg", Stock = 40, CategoryId = kidsCat.Id, BrandId = legoBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "عروسة Barbie Fashionista مع ملحقات", Description = "عروسة Barbie Fashionista مع ملحقات", Price = 599m, OldPrice = 750m, PictureUrl = "images/barbie.jpg", Stock = 60, CategoryId = kidsCat.Id, BrandId = barbieBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "روبوت ذكاء اصطناعي تعليمي للأطفال", Description = "روبوت ذكاء اصطناعي تعليمي للأطفال", Price = 2199m, OldPrice = 2500m, PictureUrl = "images/robot.jpg", Stock = 25, CategoryId = kidsCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "دراجة هوائية للأطفال 16 بوصة", Description = "دراجة هوائية للأطفال 16 بوصة", Price = 1499m, OldPrice = 2000m, PictureUrl = "images/bike.jpg", Stock = 15, CategoryId = kidsCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "طقم رسم وتلوين احترافي للأطفال 48 قلم", Description = "طقم رسم وتلوين احترافي للأطفال 48 قلم", Price = 399m, OldPrice = 499m, PictureUrl = "images/colors.jpg", Stock = 100, CategoryId = kidsCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Product { Name = "حوض سباحة منزلي للأطفال 300×180 سم", Description = "حوض سباحة منزلي للأطفال 300×180 سم", Price = 899m, OldPrice = 1100m, PictureUrl = "images/pool.jpg", Stock = 30, CategoryId = kidsCat.Id, BrandId = otherBrand.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                    };

                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();

                    // 5. Add Product Images
                    foreach (var product in products)
                    {
                        var images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = product.PictureUrl ?? string.Empty, IsMain = true, ProductId = product.Id },
                        };
                        await context.ProductImages.AddRangeAsync(images);
                    }
                    await context.SaveChangesAsync();
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
