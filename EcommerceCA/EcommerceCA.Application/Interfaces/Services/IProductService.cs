using EcommerceCA.Application.DTOs.Product;

namespace EcommerceCA.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductResponseDto>        CreateAsync(CreateProductDto dto);
    Task<PaginatedProductResult>    GetAllAsync(ProductQueryParams query);
    Task<ProductResponseDto>        GetByIdAsync(int id);
    Task<ProductResponseDto>        UpdateAsync(int id, UpdateProductDto dto);
    Task                            DeleteAsync(int id);
    Task<ProductVariantResponseDto> AddVariantAsync(int productId, CreateProductVariantDto dto);
    Task                            DeleteVariantAsync(int variantId);
}
