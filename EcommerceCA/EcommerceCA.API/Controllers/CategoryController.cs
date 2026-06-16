using EcommerceCA.Application.DTOs.Category;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Constants;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCA.API.Controllers;

/// <summary>Category management — CRUD + image upload</summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoryController(ICategoryService categoryService) => _categoryService = categoryService;

    /// <summary>Get all active categories</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryResponseDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<List<CategoryResponseDto>>.Ok(result));
    }

    /// <summary>Get a single category by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(ApiResponse<CategoryResponseDto>.Ok(result));
    }

    /// <summary>Create a new category with optional image upload — Admin only</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return StatusCode(201, ApiResponse<CategoryResponseDto>.Created(result));
    }

    /// <summary>Update an existing category — Admin only</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        return Ok(ApiResponse<CategoryResponseDto>.Ok(result, "Category updated successfully."));
    }

    /// <summary>Delete a category — Admin only (cannot delete if it has active products)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Category deleted successfully."));
    }
}
