using EcommerceCA.Application.DTOs.Cart;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceCA.API.Controllers;

/// <summary>Shopping cart — add, update, remove items and view cart</summary>
[ApiController]
[Route("api/cart")]
[Authorize]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService) => _cartService = cartService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Get the current user's cart</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartResponseDto>), 200)]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCartAsync(CurrentUserId);
        return Ok(ApiResponse<CartResponseDto>.Ok(result));
    }

    /// <summary>Add a product (or variant) to the cart</summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var result = await _cartService.AddToCartAsync(CurrentUserId, dto);
        return Ok(ApiResponse<CartResponseDto>.Ok(result, "Item added to cart."));
    }

    /// <summary>Update the quantity of a cart item</summary>
    [HttpPut("items/{cartItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CartResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var result = await _cartService.UpdateCartItemAsync(CurrentUserId, cartItemId, dto);
        return Ok(ApiResponse<CartResponseDto>.Ok(result, "Cart item updated."));
    }

    /// <summary>Remove a specific item from the cart</summary>
    [HttpDelete("items/{cartItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CartResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var result = await _cartService.RemoveFromCartAsync(CurrentUserId, cartItemId);
        return Ok(ApiResponse<CartResponseDto>.Ok(result, "Item removed from cart."));
    }

    /// <summary>Clear all items from the cart</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(CurrentUserId);
        return Ok(ApiResponse<object>.Ok(null!, "Cart cleared."));
    }
}
