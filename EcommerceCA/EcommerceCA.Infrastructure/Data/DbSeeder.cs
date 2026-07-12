using EcommerceCA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceCA.Infrastructure.Data;

public static class DbSeeder
{
    public static void SeedData(ApplicationDbContext context)
    {
        // Baseline existing migrations history if __EFMigrationsHistory is empty/missing
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END;

                IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260620130003_InitialCreate')
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260620130003_InitialCreate', '9.0.3');

                IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260621113938_UpdateSeedAndDecimal')
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260621113938_UpdateSeedAndDecimal', '9.0.3');
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Warning: Baseline migration sync skipped or failed. " + ex.Message);
        }

        // ── Ensure Admin User Is Updated ──────────────────────────────────────
        var seededAdmin = context.Set<ApplicationUser>().FirstOrDefault(u => u.Id == "seed-user-admin" || u.Email == "admin@yourchoice.com" || u.Email == "admin@yourstore.com");
        if (seededAdmin != null && (seededAdmin.Email != "admin@yourstore.com" || seededAdmin.PhoneNumber != "9999999999"))
        {
            seededAdmin.Email = "admin@yourstore.com";
            seededAdmin.NormalizedEmail = "ADMIN@YOURSTORE.COM";
            seededAdmin.UserName = "admin@yourstore.com";
            seededAdmin.NormalizedUserName = "ADMIN@YOURSTORE.COM";
            seededAdmin.FirstName = "Super";
            seededAdmin.LastName = "Admin";
            seededAdmin.PhoneNumber = "9999999999";
            
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
            seededAdmin.PasswordHash = passwordHasher.HashPassword(seededAdmin, "Admin@1234");
            
            context.SaveChanges();
        }

        // ── Ensure Admin User Has Admin Role ──────────────────────────────────
        if (seededAdmin != null)
        {
            var adminRoleId = "seed-role-admin";
            var userRoleExists = context.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                .Any(ur => ur.UserId == seededAdmin.Id && ur.RoleId == adminRoleId);
            if (!userRoleExists)
            {
                context.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>
                {
                    UserId = seededAdmin.Id,
                    RoleId = adminRoleId
                });
                context.SaveChanges();
            }
        }

        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Personalized Mugs", Description = "Custom ceramic mugs", ImageUrl = "https://images.unsplash.com/photo-1517254456776-9bb245d2b843?auto=format&fit=crop&w=400&h=400&q=80" },
                new Category { Name = "Custom Cushions", Description = "Soft custom cushions", ImageUrl = "https://images.unsplash.com/photo-1584100936595-c0654b55a2e2?auto=format&fit=crop&w=400&h=400&q=80" },
                new Category { Name = "Photo Cakes", Description = "Delicious photo cakes", ImageUrl = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?auto=format&fit=crop&w=400&h=400&q=80" },
                new Category { Name = "Flower Bouquets", Description = "Beautiful flower bouquets", ImageUrl = "https://images.unsplash.com/photo-1526047932273-341f2a7631f9?auto=format&fit=crop&w=400&h=400&q=80" },
                new Category { Name = "Unique Jewelry", Description = "Elegant personalized jewelry", ImageUrl = "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=400&h=400&q=80" },
                new Category { Name = "Personalized Frames", Description = "Custom engraved frames", ImageUrl = "https://images.unsplash.com/photo-1583847268964-b28dc8f51f92?auto=format&fit=crop&w=400&h=400&q=80" },
                new Category { Name = "Personalized Wallets", Description = "Handcrafted custom wallets", ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?auto=format&fit=crop&w=400&h=400&q=80" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            var mugs = context.Categories.First(c => c.Name == "Personalized Mugs");
            var cushions = context.Categories.First(c => c.Name == "Custom Cushions");
            var cakes = context.Categories.First(c => c.Name == "Photo Cakes");
            var bouquets = context.Categories.First(c => c.Name == "Flower Bouquets");
            var jewelry = context.Categories.First(c => c.Name == "Unique Jewelry");
            var frames = context.Categories.First(c => c.Name == "Personalized Frames");
            var wallets = context.Categories.First(c => c.Name == "Personalized Wallets");

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Magic Color Changing Mug",
                    Price = 399,
                    DiscountPrice = 599,
                    ImageUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "Add your favorite photo. The photo appears when hot liquid is poured into the mug. High-quality ceramic with heat-sensitive coating.",
                    Stock = 100,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = mugs.Id
                },
                new Product
                {
                    Name = "Soft Personalized Cushion",
                    Price = 499,
                    ImageUrl = "https://images.unsplash.com/photo-1592078615290-033ee584e267?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "Beautifully printed soft cushion with your precious memories. Perfect for sofa or bed decor.",
                    Stock = 50,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = cushions.Id
                },
                new Product
                {
                    Name = "Heart Shaped Red Velvet Cake",
                    Price = 799,
                    DiscountPrice = 999,
                    ImageUrl = "https://images.unsplash.com/photo-1586985289688-ca3cf47d3e6e?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "Delicious Red Velvet cake in heart shape, perfect for anniversaries. Eggless option available.",
                    Stock = 30,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = cakes.Id
                },
                new Product
                {
                    Name = "Royal Rose & Lily Bouquet",
                    Price = 1299,
                    ImageUrl = "https://images.unsplash.com/photo-1561101210-af96fd792040?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "A premium mix of red roses and white lilies wrapped elegantly. Comes with a personalized card.",
                    Stock = 40,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = bouquets.Id
                },
                new Product
                {
                    Name = "Initial Engraved Gold Pendant",
                    Price = 2499,
                    DiscountPrice = 2999,
                    ImageUrl = "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "Dainty gold pendant engraved with your initial. 18k gold plated. Includes adjustable chain.",
                    Stock = 25,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = jewelry.Id
                },
                new Product
                {
                    Name = "Rotating Photo Lamp",
                    Price = 1599,
                    ImageUrl = "https://images.unsplash.com/photo-1534073828943-f801091bb18c?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "A glowing lamp that rotates and showcases your favorite 4 photos. USB powered with LED lights.",
                    Stock = 35,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = frames.Id
                },
                new Product
                {
                    Name = "Personalized Wooden Plaque",
                    Price = 899,
                    ImageUrl = "https://images.unsplash.com/photo-1544450173-8c8728a13c90?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "Laser engraved wooden plaque with your favorite quote or image. Sustainable pine wood.",
                    Stock = 45,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = frames.Id
                },
                new Product
                {
                    Name = "Custom Couple Keychain",
                    Price = 249,
                    ImageUrl = "https://images.unsplash.com/photo-1622560853942-64b21c56ac11?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "A pair of interlocking keychains with engraved names. Zinc alloy material.",
                    Stock = 120,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = jewelry.Id
                },
                new Product
                {
                    Name = "Handcrafted Personalized Leather Wallet",
                    Price = 1299,
                    DiscountPrice = 1599,
                    ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?auto=format&fit=crop&w=400&h=400&q=80",
                    Description = "Premium vegan leather wallet with dual-compartment for cards and cash. Name engraving available on the bottom right corner.",
                    Stock = 60,
                    IsActive = true,
                    IsCustomizable = true,
                    CategoryId = wallets.Id
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }

        // ── Seed specific 5 requested products ──
        var jewelryCat = context.Categories.FirstOrDefault(c => c.Name == "Jewelry");
        if (jewelryCat == null)
        {
            jewelryCat = new Category { Name = "Jewelry", Description = "Custom jewelry and pendants", ImageUrl = "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=400&h=400&q=80" };
            context.Categories.Add(jewelryCat);
            context.SaveChanges();
        }

        var homeDecorCat = context.Categories.FirstOrDefault(c => c.Name == "Home Decor");
        if (homeDecorCat == null)
        {
            homeDecorCat = new Category { Name = "Home Decor", Description = "Custom home decor and lights", ImageUrl = "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?auto=format&fit=crop&w=400&h=400&q=80" };
            context.Categories.Add(homeDecorCat);
            context.SaveChanges();
        }

        var newProds = new List<Product>();

        if (!context.Products.Any(p => p.Name == "Personalized Name Necklace"))
        {
            newProds.Add(new Product
            {
                Name = "Personalized Name Necklace",
                Price = 899,
                DiscountPrice = 599,
                ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?auto=format&fit=crop&w=600&h=600&q=80",
                Description = "Beautiful sterling silver necklace with your name engraved. Perfect gift for loved ones on any occasion. __CUSTOM_PRODUCT_DATA__:{\"variants\":[],\"isCustomizationAvailable\":true,\"customizationTypes\":[\"text\"],\"customizationInstructions\":\"Enter the name to be engraved (max 10 characters)\",\"images\":[\"https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?auto=format&fit=crop&w=600&h=600&q=80\",\"https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=600&h=600&q=80\"]}",
                Stock = 50,
                IsActive = true,
                IsCustomizable = true,
                CategoryId = jewelryCat.Id
            });
        }

        if (!context.Products.Any(p => p.Name == "Custom Printed Photo Mug"))
        {
            newProds.Add(new Product
            {
                Name = "Custom Printed Photo Mug",
                Price = 599,
                DiscountPrice = 399,
                ImageUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=600&h=600&q=80",
                Description = "High quality ceramic mug printed with your favorite photo. Microwave and dishwasher safe. 330ml capacity. __CUSTOM_PRODUCT_DATA__:{\"variants\":[],\"isCustomizationAvailable\":true,\"customizationTypes\":[\"image\",\"text\"],\"customizationInstructions\":\"Upload your photo and add a message (max 30 characters)\",\"images\":[\"https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=600&h=600&q=80\",\"https://images.unsplash.com/photo-1576092768241-dec231879fc3?auto=format&fit=crop&w=600&h=600&q=80\"]}",
                Stock = 100,
                IsActive = true,
                IsCustomizable = true,
                CategoryId = homeDecorCat.Id
            });
        }

        if (!context.Products.Any(p => p.Name == "Engraved Wooden Keepsake Box"))
        {
            newProds.Add(new Product
            {
                Name = "Engraved Wooden Keepsake Box",
                Price = 1499,
                DiscountPrice = 999,
                ImageUrl = "https://images.unsplash.com/photo-1532634922-8fe0b757fb13?auto=format&fit=crop&w=600&h=600&q=80",
                Description = "Handcrafted wooden box with custom engraving. Perfect for storing memories, jewelry, and precious keepsakes. __CUSTOM_PRODUCT_DATA__:{\"variants\":[],\"isCustomizationAvailable\":true,\"customizationTypes\":[\"text\"],\"customizationInstructions\":\"Enter text to be engraved on lid (max 20 characters)\",\"images\":[\"https://images.unsplash.com/photo-1532634922-8fe0b757fb13?auto=format&fit=crop&w=600&h=600&q=80\",\"https://images.unsplash.com/photo-1607344645866-009c320c5ab8?auto=format&fit=crop&w=600&h=600&q=80\"]}",
                Stock = 30,
                IsActive = true,
                IsCustomizable = true,
                CategoryId = homeDecorCat.Id
            });
        }

        if (!context.Products.Any(p => p.Name == "Scented Soy Candle Set"))
        {
            newProds.Add(new Product
            {
                Name = "Scented Soy Candle Set",
                Price = 799,
                DiscountPrice = 549,
                ImageUrl = "https://images.unsplash.com/photo-1603006905003-be475563bc59?auto=format&fit=crop&w=600&h=600&q=80",
                Description = "Set of 3 handpoured soy wax candles in lavender, vanilla, and rose scents. Burns for up to 40 hours each. __CUSTOM_PRODUCT_DATA__:{\"variants\":[],\"isCustomizationAvailable\":false,\"customizationTypes\":[],\"customizationInstructions\":\"\",\"images\":[\"https://images.unsplash.com/photo-1603006905003-be475563bc59?auto=format&fit=crop&w=600&h=600&q=80\",\"https://images.unsplash.com/photo-1602872030219-5fb6fa999157?auto=format&fit=crop&w=600&h=600&q=80\"]}",
                Stock = 75,
                IsActive = true,
                IsCustomizable = false,
                CategoryId = homeDecorCat.Id
            });
        }

        if (!context.Products.Any(p => p.Name == "Couple Matching Bracelet Set"))
        {
            newProds.Add(new Product
            {
                Name = "Couple Matching Bracelet Set",
                Price = 699,
                DiscountPrice = 449,
                ImageUrl = "https://images.unsplash.com/photo-1611591437281-460bfbe1220a?auto=format&fit=crop&w=600&h=600&q=80",
                Description = "Beautiful matching bracelet set for couples. Made with premium stainless steel. Waterproof and tarnish resistant. Comes in a gift box. __CUSTOM_PRODUCT_DATA__:{\"variants\":[],\"isCustomizationAvailable\":false,\"customizationTypes\":[],\"customizationInstructions\":\"\",\"images\":[\"https://images.unsplash.com/photo-1611591437281-460bfbe1220a?auto=format&fit=crop&w=600&h=600&q=80\",\"https://images.unsplash.com/photo-1573408301185-9146fe634ad0?auto=format&fit=crop&w=600&h=600&q=80\"]}",
                Stock = 60,
                IsActive = true,
                IsCustomizable = false,
                CategoryId = jewelryCat.Id
            });
        }

        if (newProds.Any())
        {
            context.Products.AddRange(newProds);
            context.SaveChanges();
        }
    }
}
