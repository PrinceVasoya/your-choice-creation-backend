using AutoMapper;
using EcommerceCA.Application.DTOs.Product;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;

namespace EcommerceCA.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository  _repo;
    private readonly ICategoryRepository _catRepo;
    private readonly IImageService       _imageService;
    private readonly IMapper             _mapper;

    public ProductService(
        IProductRepository  repo,
        ICategoryRepository catRepo,
        IImageService       imageService,
        IMapper             mapper)
    {
        _repo         = repo;
        _catRepo      = catRepo;
        _imageService = imageService;
        _mapper       = mapper;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        if (await _catRepo.GetByIdAsync(dto.CategoryId) == null)
            throw new NotFoundException("Category", dto.CategoryId);

        var product = new Product
        {
            Name           = dto.Name,
            ProductCode    = string.IsNullOrWhiteSpace(dto.ProductCode) ? GenerateProductCode() : dto.ProductCode.Trim(),
            Description    = dto.Description,
            Price          = dto.Price,
            DiscountPrice  = dto.DiscountPrice,
            Stock          = dto.Stock,
            CategoryId     = dto.CategoryId,
            IsCustomizable = dto.IsCustomizable,
            ImageUrl       = dto.ImageUrl
        };

        if (dto.Image != null)
        {
            var (url, pid) = await _imageService.UploadAsync(dto.Image, "products");
            product.ImageUrl      = url;
            product.ImagePublicId = pid;
        }

        if (dto.Variants != null && dto.Variants.Any())
        {
            product.Variants = dto.Variants.Select(v => new ProductVariant
            {
                Size            = v.Size,
                Color           = v.Color,
                ColorHex        = v.ColorHex,
                PriceAdjustment = v.PriceAdjustment,
                Stock           = v.Stock,
                SKU             = v.SKU
            }).ToList();
        }

        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetWithDetailsAsync(product.Id);
        return _mapper.Map<ProductResponseDto>(created);
    }

    public async Task<PaginatedProductResult> GetAllAsync(ProductQueryParams query)
    {
        var (items, total) = await _repo.GetPagedAsync(
            query.Page, query.PageSize,
            query.Search, query.CategoryId,
            query.MinPrice, query.MaxPrice,
            query.IsCustomizable, query.InStock,
            query.SortBy, query.SortOrder);

        return new PaginatedProductResult
        {
            Items      = _mapper.Map<List<ProductResponseDto>>(items),
            TotalCount = total,
            Page       = query.Page,
            PageSize   = query.PageSize
        };
    }

    public async Task<ProductResponseDto> GetByIdAsync(int id)
    {
        var product = await _repo.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Product", id);
        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _repo.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Product", id);

        if (dto.Name != null)           product.Name           = dto.Name;
        if (dto.ProductCode != null)    product.ProductCode    = dto.ProductCode.Trim();
        if (dto.Description != null)    product.Description    = dto.Description;
        if (dto.Price.HasValue)         product.Price          = dto.Price.Value;
        if (dto.DiscountPrice.HasValue) product.DiscountPrice  = dto.DiscountPrice.Value;
        if (dto.Stock.HasValue)         product.Stock          = dto.Stock.Value;
        if (dto.CategoryId.HasValue)    product.CategoryId     = dto.CategoryId.Value;
        if (dto.IsActive.HasValue)      product.IsActive       = dto.IsActive.Value;
        if (dto.IsCustomizable.HasValue)product.IsCustomizable = dto.IsCustomizable.Value;
        if (dto.ImageUrl != null)       product.ImageUrl       = dto.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        if (dto.Image != null)
        {
            if (product.ImagePublicId != null)
                await _imageService.DeleteAsync(product.ImagePublicId);
            var (url, pid) = await _imageService.UploadAsync(dto.Image, "products");
            product.ImageUrl      = url;
            product.ImagePublicId = pid;
        }

        await _repo.UpdateAsync(product);
        await _repo.SaveChangesAsync();

        return _mapper.Map<ProductResponseDto>(await _repo.GetWithDetailsAsync(id));
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        product.IsActive  = false;   // Soft delete
        product.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(product);
        await _repo.SaveChangesAsync();
    }

    public async Task<ProductVariantResponseDto> AddVariantAsync(int productId, CreateProductVariantDto dto)
    {
        var product = await _repo.GetByIdAsync(productId)
            ?? throw new NotFoundException("Product", productId);

        var variant = new ProductVariant
        {
            ProductId       = productId,
            Size            = dto.Size,
            Color           = dto.Color,
            ColorHex        = dto.ColorHex,
            PriceAdjustment = dto.PriceAdjustment,
            Stock           = dto.Stock,
            SKU             = dto.SKU
        };

        product.Variants.Add(variant);
        await _repo.UpdateAsync(product);
        await _repo.SaveChangesAsync();

        return _mapper.Map<ProductVariantResponseDto>(variant);
    }

    public async Task DeleteVariantAsync(int variantId)
    {
        var variant = await _repo.GetVariantAsync(variantId)
            ?? throw new NotFoundException("ProductVariant", variantId);

        variant.IsActive = false;
        await _repo.SaveChangesAsync();
    }

    private string GenerateProductCode()
    {
        return $"PRD-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
    }
}
