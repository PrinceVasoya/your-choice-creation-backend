using AutoMapper;
using EcommerceCA.Application.DTOs.Cart;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;

namespace EcommerceCA.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository    _cartRepo;
    private readonly IProductRepository _productRepo;
    private readonly IMapper            _mapper;

    public CartService(
        ICartRepository    cartRepo,
        IProductRepository productRepo,
        IMapper            mapper)
    {
        _cartRepo    = cartRepo;
        _productRepo = productRepo;
        _mapper      = mapper;
    }

    public async Task<CartResponseDto> GetCartAsync(string userId)
    {
        var cart = await _cartRepo.GetOrCreateAsync(userId);
        return _mapper.Map<CartResponseDto>(cart);
    }

    public async Task<CartResponseDto> AddToCartAsync(string userId, AddToCartDto dto)
    {
        var product = await _productRepo.GetByIdAsync(dto.ProductId)
            ?? throw new NotFoundException("Product", dto.ProductId);

        if (!product.IsActive)
            throw new BadRequestException($"Product '{product.Name}' is no longer available.");

        if (dto.ProductVariantId.HasValue)
        {
            var variant = await _productRepo.GetVariantAsync(dto.ProductVariantId.Value)
                ?? throw new NotFoundException("ProductVariant", dto.ProductVariantId.Value);

            if (variant.ProductId != dto.ProductId)
                throw new BadRequestException("The selected variant does not belong to this product.");
        }

        var cart = await _cartRepo.GetOrCreateAsync(userId);

        // If same product+variant already in cart, increment quantity
        var existing = cart.CartItems.FirstOrDefault(ci =>
            ci.ProductId == dto.ProductId &&
            ci.ProductVariantId == dto.ProductVariantId);

        if (existing != null)
        {
            if (product.Stock < existing.Quantity + dto.Quantity)
                throw new BadRequestException($"Cannot add to cart. Only {product.Stock} units of '{product.Name}' are available, and you already have {existing.Quantity} in your cart.");

            existing.Quantity  += dto.Quantity;
            existing.UpdatedAt  = DateTime.UtcNow;
        }
        else
        {
            if (product.Stock < dto.Quantity)
                throw new BadRequestException($"Cannot add to cart. Only {product.Stock} units of '{product.Name}' are available.");

            var newItem = new CartItem
            {
                CartId            = cart.Id,
                ProductId         = dto.ProductId,
                ProductVariantId  = dto.ProductVariantId,
                Quantity          = dto.Quantity,
                CustomizationNote = dto.CustomizationNote
            };
            cart.CartItems.Add(newItem);
            await _cartRepo.AddItemAsync(newItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepo.SaveChangesAsync();

        var updated = await _cartRepo.GetWithItemsAsync(userId);
        return _mapper.Map<CartResponseDto>(updated);
    }

    public async Task<CartResponseDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto dto)
    {
        var cart = await _cartRepo.GetOrCreateAsync(userId);

        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId)
            ?? throw new NotFoundException("CartItem", cartItemId);

        var product = await _productRepo.GetByIdAsync(item.ProductId)
            ?? throw new NotFoundException("Product", item.ProductId);

        if (product.Stock < dto.Quantity)
            throw new BadRequestException($"Cannot update quantity. Only {product.Stock} units of '{product.Name}' are available.");

        item.Quantity  = dto.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepo.SaveChangesAsync();

        var updated = await _cartRepo.GetWithItemsAsync(userId);
        return _mapper.Map<CartResponseDto>(updated);
    }

    public async Task<CartResponseDto> RemoveFromCartAsync(string userId, int cartItemId)
    {
        var cart = await _cartRepo.GetOrCreateAsync(userId);

        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId)
            ?? throw new NotFoundException("CartItem", cartItemId);

        await _cartRepo.RemoveItemAsync(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepo.SaveChangesAsync();

        var updated = await _cartRepo.GetWithItemsAsync(userId);
        return _mapper.Map<CartResponseDto>(updated);
    }

    public async Task ClearCartAsync(string userId)
    {
        await _cartRepo.ClearAsync(userId);
        await _cartRepo.SaveChangesAsync();
    }
}
