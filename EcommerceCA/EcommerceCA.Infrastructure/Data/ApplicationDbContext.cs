using EcommerceCA.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Category>       Categories      => Set<Category>();
    public DbSet<Product>        Products        => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Cart>           Carts           => Set<Cart>();
    public DbSet<CartItem>       CartItems       => Set<CartItem>();
    public DbSet<Order>          Orders          => Set<Order>();
    public DbSet<OrderItem>      OrderItems      => Set<OrderItem>();
    public DbSet<Payment>        Payments        => Set<Payment>();
    public DbSet<Address>        Addresses       => Set<Address>();
    public DbSet<RefreshToken>   RefreshTokens   => Set<RefreshToken>();
    public DbSet<WishlistItem>   WishlistItems   => Set<WishlistItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ───────────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.HasMany(u => u.Addresses).WithOne(a => a.User)
                .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(u => u.Cart).WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(u => u.Orders).WithOne(o => o.User)
                .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(u => u.RefreshTokens).WithOne(r => r.User)
                .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Category ──────────────────────────────────────────────────────────
        builder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(150).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.HasIndex(c => c.Name).IsUnique();
            e.HasMany(c => c.Products).WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Product ───────────────────────────────────────────────────────────
        builder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.ProductCode).HasMaxLength(50).IsRequired().HasDefaultValue("TEMP_CODE");
            e.HasIndex(p => p.ProductCode).IsUnique();
            e.Property(p => p.Description).HasMaxLength(2000);
            e.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(p => p.DiscountPrice).HasColumnType("decimal(18,2)");
            e.HasMany(p => p.Variants).WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── ProductVariant ────────────────────────────────────────────────────
        builder.Entity<ProductVariant>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Size).HasMaxLength(50);
            e.Property(v => v.Color).HasMaxLength(100);
            e.Property(v => v.ColorHex).HasMaxLength(10);
            e.Property(v => v.SKU).HasMaxLength(100);
            e.Property(v => v.PriceAdjustment).HasColumnType("decimal(18,2)");
        });

        // ── Cart ──────────────────────────────────────────────────────────────
        builder.Entity<Cart>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasMany(c => c.CartItems).WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartItem>(e =>
        {
            e.HasKey(ci => ci.Id);
            e.HasOne(ci => ci.Product).WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ci => ci.ProductVariant).WithMany()
                .HasForeignKey(ci => ci.ProductVariantId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Order ─────────────────────────────────────────────────────────────
        builder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
            e.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.GrandTotal).HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).HasConversion<string>();
            e.HasMany(o => o.OrderItems).WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.ShippingAddress).WithMany(a => a.Orders)
                .HasForeignKey(o => o.ShippingAddressId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Payment).WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── OrderItem ─────────────────────────────────────────────────────────
        builder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)");
            e.HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(oi => oi.ProductVariant).WithMany()
                .HasForeignKey(oi => oi.ProductVariantId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Payment ───────────────────────────────────────────────────────────
        builder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.Method).HasConversion<string>();
        });

        // ── Address ───────────────────────────────────────────────────────────
        builder.Entity<Address>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FullName).HasMaxLength(150).IsRequired();
            e.Property(a => a.Phone).HasMaxLength(20).IsRequired();
            e.Property(a => a.AddressLine1).HasMaxLength(300).IsRequired();
            e.Property(a => a.City).HasMaxLength(100).IsRequired();
            e.Property(a => a.State).HasMaxLength(100).IsRequired();
            e.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        });

        // ── RefreshToken ──────────────────────────────────────────────────────
        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
        });

        // ── Wishlist ──────────────────────────────────────────────────────────
        builder.Entity<WishlistItem>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            e.HasOne(w => w.User).WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(w => w.Product).WithMany(p => p.WishlistItems)
                .HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed Roles ────────────────────────────────────────────────────────
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "seed-role-admin", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
            new IdentityRole { Id = "seed-role-user",  Name = "User",  NormalizedName = "USER",  ConcurrencyStamp = "2" }
        );

        // ── Seed Admin User ──────────────────────────────────────────────────
        var adminUser = new ApplicationUser
        {
            Id = "seed-user-admin",
            FirstName = "Super",
            LastName = "Admin",
            Email = "admin@yourstore.com",
            NormalizedEmail = "ADMIN@YOURSTORE.COM",
            UserName = "admin@yourstore.com",
            NormalizedUserName = "ADMIN@YOURSTORE.COM",
            PhoneNumber = "9999999999",
            EmailConfirmed = true,
            SecurityStamp = "seed-security-stamp",
            ConcurrencyStamp = "seed-concurrency-stamp",
            CreatedAt = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
        };

        adminUser.PasswordHash = "AQAAAAIAAYagAAAAEGSwk2grLuyQCyk8zt1BJ/FMmpsyiobc0jS4F3no0KDF9sWHAy9RYF4thm1/U6YG0Q==";

        builder.Entity<ApplicationUser>().HasData(adminUser);

        // ── Seed User Role Mapping ──────────────────────────────────────────
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "seed-role-admin",
                UserId = "seed-user-admin"
            }
        );
    }
}
