using EcommerceCA.Application.DTOs.Cart;

namespace EcommerceCA.Application.Interfaces.Services;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(string userId);
    Task<CartResponseDto> AddToCartAsync(string userId, AddToCartDto dto);
    Task<CartResponseDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto dto);
    Task<CartResponseDto> RemoveFromCartAsync(string userId, int cartItemId);
    Task                  ClearCartAsync(string userId);
}
