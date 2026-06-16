using EcommerceCA.Application.DTOs.Product;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EcommerceCA.API.Controllers;

/// <summary>User wishlist management — add, remove, and retrieve saved items</summary>
[ApiController]
[Route("api/wishlist")]
[Authorize]
[Produces("application/json")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    public WishlistController(IWishlistService wishlistService) => _wishlistService = wishlistService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Get the current user's wishlist products</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductResponseDto>>), 200)]
    public async Task<IActionResult> GetWishlist()
    {
        var result = await _wishlistService.GetWishlistAsync(CurrentUserId);
        return Ok(ApiResponse<List<ProductResponseDto>>.Ok(result));
    }

    /// <summary>Get a list of product IDs in the current user's wishlist</summary>
    [HttpGet("ids")]
    [ProducesResponseType(typeof(ApiResponse<List<int>>), 200)]
    public async Task<IActionResult> GetWishlistProductIds()
    {
        var result = await _wishlistService.GetWishlistProductIdsAsync(CurrentUserId);
        return Ok(ApiResponse<List<int>>.Ok(result));
    }

    /// <summary>Add a product to the user's wishlist</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AddToWishlist([FromBody] AddWishlistRequestDto dto)
    {
        await _wishlistService.AddToWishlistAsync(CurrentUserId, dto.ProductId);
        return Ok(ApiResponse<object>.Ok(null!, "Product added to wishlist successfully."));
    }

    /// <summary>Remove a product from the user's wishlist</summary>
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var removed = await _wishlistService.RemoveFromWishlistAsync(CurrentUserId, productId);
        if (!removed)
        {
            return NotFound(ApiResponse<object>.Fail("Product was not in your wishlist."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Product removed from wishlist successfully."));
    }
}

public class AddWishlistRequestDto
{
    public int ProductId { get; set; }
}
