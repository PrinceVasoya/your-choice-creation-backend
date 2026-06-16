using EcommerceCA.Application.DTOs.Product;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Constants;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCA.API.Controllers;

/// <summary>Product management — CRUD, pagination, filtering, sorting, variants</summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IImageService   _imageService;

    public ProductController(IProductService productService, IImageService imageService)
    {
        _productService = productService;
        _imageService   = imageService;
    }

    /// <summary>
    /// Get all products with pagination, filtering, and sorting.
    /// Supports: search, categoryId, minPrice, maxPrice, isCustomizable, inStock,
    /// sortBy (price|name|createdAt), sortOrder (asc|desc)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ProductResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams query)
    {
        var result = await _productService.GetAllAsync(query);
        return Ok(new PaginatedResponse<ProductResponseDto>
        {
            Data = result.Items,
            Meta = new PaginationMeta
            {
                TotalCount  = result.TotalCount,
                Page        = result.Page,
                PageSize    = result.PageSize,
                TotalPages  = result.TotalPages,
                HasPrevious = result.HasPrevious,
                HasNext     = result.HasNext
            }
        });
    }

    /// <summary>Get a single product with its variants</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse<ProductResponseDto>.Ok(result));
    }

    /// <summary>Create a new product with optional image and variants — Admin only</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        return StatusCode(201, ApiResponse<ProductResponseDto>.Created(result));
    }

    /// <summary>Update an existing product — Admin only</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ProductResponseDto>.Ok(result, "Product updated successfully."));
    }

    /// <summary>Soft-delete a product (sets IsActive = false) — Admin only</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Product deleted successfully."));
    }

    /// <summary>Add a size/color variant to a product — Admin only</summary>
    [HttpPost("{productId:int}/variants")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ProductVariantResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> AddVariant(int productId, [FromBody] CreateProductVariantDto dto)
    {
        var result = await _productService.AddVariantAsync(productId, dto);
        return StatusCode(201, ApiResponse<ProductVariantResponseDto>.Created(result, "Variant added."));
    }

    /// <summary>Soft-delete a product variant — Admin only</summary>
    [HttpDelete("variants/{variantId:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteVariant(int variantId)
    {
        await _productService.DeleteVariantAsync(variantId);
        return Ok(ApiResponse<object>.Ok(null!, "Variant removed."));
    }

    /// <summary>Upload a single product image — Admin only</summary>
    [HttpPost("upload-image")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No file uploaded."));

        var (url, publicId) = await _imageService.UploadAsync(file, "products");
        return Ok(ApiResponse<object>.Ok(new { url, publicId }));
    }
}
