using AutoMapper;
using EcommerceCA.Application.DTOs.Cart;
using EcommerceCA.Application.DTOs.Category;
using EcommerceCA.Application.DTOs.Order;
using EcommerceCA.Application.DTOs.Payment;
using EcommerceCA.Application.DTOs.Product;
using EcommerceCA.Application.DTOs.User;
using EcommerceCA.Domain.Entities;

namespace EcommerceCA.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── Category ──────────────────────────────────────────────────────────
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(d => d.ProductCount,
                o => o.MapFrom(s => s.Products.Count(p => p.IsActive)));

        // ── Product ───────────────────────────────────────────────────────────
        CreateMap<Product, ProductResponseDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.Variants,
                o => o.MapFrom(s => s.Variants.Where(v => v.IsActive)));

        CreateMap<ProductVariant, ProductVariantResponseDto>();

        // ── Cart ──────────────────────────────────────────────────────────────
        CreateMap<CartItem, CartItemResponseDto>()
            .ForMember(d => d.ProductName,
                o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductImageUrl,
                o => o.MapFrom(s => s.Product.ImageUrl))
            .ForMember(d => d.UnitPrice,
                o => o.MapFrom(s => CalculateUnitPrice(s)))
            .ForMember(d => d.TotalPrice,
                o => o.MapFrom(s => CalculateUnitPrice(s) * s.Quantity))
            .ForMember(d => d.VariantInfo,
                o => o.MapFrom(s => BuildVariantInfo(s.ProductVariant)));

        CreateMap<Cart, CartResponseDto>()
            .ForMember(d => d.SubTotal,
                o => o.MapFrom(s => s.CartItems.Sum(ci => CalculateUnitPrice(ci) * ci.Quantity)))
            .ForMember(d => d.ItemCount,
                o => o.MapFrom(s => s.CartItems.Sum(ci => ci.Quantity)))
            .ForMember(d => d.Items,
                o => o.MapFrom(s => s.CartItems));

        // ── Order ─────────────────────────────────────────────────────────────
        CreateMap<Order, OrderResponseDto>()
            .ForMember(d => d.Status,
                o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ShippingAddress,
                o => o.MapFrom(s => s.ShippingAddress))
            .ForMember(d => d.Items,
                o => o.MapFrom(s => s.OrderItems));

        CreateMap<OrderItem, OrderItemResponseDto>()
            .ForMember(d => d.ProductImageUrl,
                o => o.MapFrom(s => s.Product.ImageUrl));

        CreateMap<Address, AddressInOrderDto>();

        CreateMap<Payment, OrderPaymentResponseDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Method, o => o.MapFrom(s => s.Method.ToString()));

        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Method, o => o.MapFrom(s => s.Method.ToString()));

        // ── User / Address ────────────────────────────────────────────────────
        CreateMap<Address, AddressResponseDto>();
        CreateMap<CreateAddressDto, Address>();

        CreateMap<ApplicationUser, UserProfileResponseDto>();
        CreateMap<ApplicationUser, AdminUserResponseDto>();
    }

    // ── Static helpers (used inside MapFrom lambdas) ──────────────────────────
    private static decimal CalculateUnitPrice(CartItem ci)
    {
        var basePrice = ci.Product?.DiscountPrice ?? ci.Product?.Price ?? 0m;
        return basePrice + (ci.ProductVariant?.PriceAdjustment ?? 0m);
    }

    private static string? BuildVariantInfo(ProductVariant? v)
    {
        if (v == null) return null;
        var parts = new List<string>();
        if (v.Size  != null) parts.Add($"Size: {v.Size}");
        if (v.Color != null) parts.Add($"Color: {v.Color}");
        return parts.Any() ? string.Join(", ", parts) : null;
    }
}
