using EcommerceCA.Application.DTOs.Category;

namespace EcommerceCA.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryResponseDto>       CreateAsync(CreateCategoryDto dto);
    Task<List<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto>       GetByIdAsync(int id);
    Task<CategoryResponseDto>       UpdateAsync(int id, UpdateCategoryDto dto);
    Task                            DeleteAsync(int id);
}
