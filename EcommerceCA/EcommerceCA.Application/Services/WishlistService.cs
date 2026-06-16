using AutoMapper;
using EcommerceCA.Application.DTOs.Product;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceCA.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepo;
    private readonly IProductRepository  _productRepo;
    private readonly IMapper             _mapper;

    public WishlistService(
        IWishlistRepository wishlistRepo,
        IProductRepository  productRepo,
        IMapper             mapper)
    {
        _wishlistRepo = wishlistRepo;
        _productRepo  = productRepo;
        _mapper       = mapper;
    }

    public async Task<List<ProductResponseDto>> GetWishlistAsync(string userId)
    {
        var items = await _wishlistRepo.GetUserWishlistAsync(userId);
        return _mapper.Map<List<ProductResponseDto>>(items.Select(w => w.Product));
    }

    public async Task<List<int>> GetWishlistProductIdsAsync(string userId)
    {
        var items = await _wishlistRepo.GetUserWishlistAsync(userId);
        return items.Select(w => w.ProductId).ToList();
    }

    public async Task<bool> AddToWishlistAsync(string userId, int productId)
    {
        var product = await _productRepo.GetByIdAsync(productId)
            ?? throw new NotFoundException("Product", productId);

        var existing = await _wishlistRepo.GetItemAsync(userId, productId);
        if (existing != null)
        {
            throw new BadRequestException("Already in wishlist");
        }

        await _wishlistRepo.AddAsync(new WishlistItem
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        });

        await _wishlistRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(string userId, int productId)
    {
        var existing = await _wishlistRepo.GetItemAsync(userId, productId);
        if (existing == null)
        {
            return false;
        }

        await _wishlistRepo.RemoveAsync(existing);
        await _wishlistRepo.SaveChangesAsync();
        return true;
    }
}
