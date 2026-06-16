using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Product;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;
    public ProductRepository(ApplicationDbContext db) => _db = db;

    public async Task<Domain.Entities.Product?> GetByIdAsync(int id) =>
        await _db.Products.FindAsync(id);

    public async Task<IEnumerable<Domain.Entities.Product>> GetAllAsync() =>
        await _db.Products.Where(p => p.IsActive).ToListAsync();

    public async Task<Domain.Entities.Product> AddAsync(Domain.Entities.Product entity)
    {
        await _db.Products.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Domain.Entities.Product entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Product entity)
    {
        _db.Products.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

    public async Task<Domain.Entities.Product?> GetWithDetailsAsync(int id) =>
        await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<(List<Domain.Entities.Product> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        string? search, int? categoryId,
        decimal? minPrice, decimal? maxPrice,
        bool? isCustomizable, bool? inStock,
        string sortBy, string sortOrder)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)));

        if (categoryId.HasValue)      query = query.Where(p => p.CategoryId     == categoryId.Value);
        if (minPrice.HasValue)        query = query.Where(p => p.Price          >= minPrice.Value);
        if (maxPrice.HasValue)        query = query.Where(p => p.Price          <= maxPrice.Value);
        if (isCustomizable.HasValue)  query = query.Where(p => p.IsCustomizable == isCustomizable.Value);
        if (inStock == true)          query = query.Where(p => p.Stock          >  0);

        query = (sortBy.ToLower(), sortOrder.ToLower()) switch
        {
            ("price",  "asc")  => query.OrderBy(p => p.Price),
            ("price",  _)      => query.OrderByDescending(p => p.Price),
            ("name",   "asc")  => query.OrderBy(p => p.Name),
            ("name",   _)      => query.OrderByDescending(p => p.Name),
            (_,        "asc")  => query.OrderBy(p => p.CreatedAt),
            _                  => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<ProductVariant?> GetVariantAsync(int variantId) =>
        await _db.ProductVariants.FindAsync(variantId);
}
