using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Wishlist;

public class WishlistRepository : IWishlistRepository
{
    private readonly ApplicationDbContext _db;
    public WishlistRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<WishlistItem>> GetUserWishlistAsync(string userId) =>
        await _db.WishlistItems
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .ToListAsync();

    public async Task<WishlistItem?> GetItemAsync(string userId, int productId) =>
        await _db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

    public async Task AddAsync(WishlistItem item)
    {
        await _db.WishlistItems.AddAsync(item);
    }

    public Task RemoveAsync(WishlistItem item)
    {
        _db.WishlistItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
