using EcommerceCA.Application.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceCA.Application.Interfaces.Services;

public interface IWishlistService
{
    Task<List<ProductResponseDto>> GetWishlistAsync(string userId);
    Task<List<int>> GetWishlistProductIdsAsync(string userId);
    Task<bool> AddToWishlistAsync(string userId, int productId);
    Task<bool> RemoveFromWishlistAsync(string userId, int productId);
}
