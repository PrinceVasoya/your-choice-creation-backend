using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Category;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public async Task<Domain.Entities.Category?> GetByIdAsync(int id) =>
        await _db.Categories.FindAsync(id);

    public async Task<IEnumerable<Domain.Entities.Category>> GetAllAsync() =>
        await _db.Categories.ToListAsync();

    public async Task<Domain.Entities.Category> AddAsync(Domain.Entities.Category entity)
    {
        await _db.Categories.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Domain.Entities.Category entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Category entity)
    {
        _db.Categories.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _db.Categories.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue) query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<Domain.Entities.Category?> GetWithProductsAsync(int id) =>
        await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Domain.Entities.Category>> GetAllWithProductCountAsync() =>
        await _db.Categories
            .Include(c => c.Products.Where(p => p.IsActive))
            .OrderBy(c => c.Name)
            .ToListAsync();
}
