using AutoMapper;
using EcommerceCA.Application.DTOs.Category;
using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Application.Interfaces.Services;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Domain.Entities;

namespace EcommerceCA.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IImageService       _imageService;
    private readonly IMapper             _mapper;

    public CategoryService(ICategoryRepository repo, IImageService imageService, IMapper mapper)
    {
        _repo         = repo;
        _imageService = imageService;
        _mapper       = mapper;
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _repo.NameExistsAsync(dto.Name))
            throw new ConflictException($"Category '{dto.Name}' already exists.");

        var category = new Category { Name = dto.Name, Description = dto.Description };

        if (dto.Image != null)
        {
            var (url, publicId) = await _imageService.UploadAsync(dto.Image, "categories");
            category.ImageUrl      = url;
            category.ImagePublicId = publicId;
        }

        await _repo.AddAsync(category);
        await _repo.SaveChangesAsync();
        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _repo.GetAllWithProductCountAsync();
        return _mapper.Map<List<CategoryResponseDto>>(categories);
    }

    public async Task<CategoryResponseDto> GetByIdAsync(int id)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);
        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        if (dto.Name != null)
        {
            if (await _repo.NameExistsAsync(dto.Name, excludeId: id))
                throw new ConflictException($"Category '{dto.Name}' already exists.");
            category.Name = dto.Name;
        }
        if (dto.Description != null) category.Description = dto.Description;
        if (dto.IsActive.HasValue) category.IsActive       = dto.IsActive.Value;
        category.UpdatedAt = DateTime.UtcNow;

        if (dto.Image != null)
        {
            if (category.ImagePublicId != null)
                await _imageService.DeleteAsync(category.ImagePublicId);

            var (url, publicId) = await _imageService.UploadAsync(dto.Image, "categories");
            category.ImageUrl      = url;
            category.ImagePublicId = publicId;
        }

        await _repo.UpdateAsync(category);
        await _repo.SaveChangesAsync();
        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _repo.GetWithProductsAsync(id)
            ?? throw new NotFoundException("Category", id);

        if (category.Products.Any(p => p.IsActive))
            throw new BadRequestException("Cannot delete a category that has active products.");

        if (category.ImagePublicId != null)
            await _imageService.DeleteAsync(category.ImagePublicId);

        await _repo.DeleteAsync(category);
        await _repo.SaveChangesAsync();
    }
}
