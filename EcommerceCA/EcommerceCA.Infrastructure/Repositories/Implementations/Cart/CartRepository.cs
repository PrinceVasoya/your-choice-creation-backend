using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Cart;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;
    public CartRepository(ApplicationDbContext db) => _db = db;

    public async Task<Domain.Entities.Cart?> GetWithItemsAsync(string userId) =>
        await _db.Carts
            .Include(c => c.User)
            .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
            .Include(c => c.CartItems).ThenInclude(ci => ci.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Domain.Entities.Cart> GetOrCreateAsync(string userId)
    {
        var cart = await GetWithItemsAsync(userId);
        if (cart != null) return cart;

        cart = new Domain.Entities.Cart { UserId = userId };
        await _db.Carts.AddAsync(cart);
        await _db.SaveChangesAsync();

        return await GetWithItemsAsync(userId) ?? cart;
    }

    public async Task AddItemAsync(CartItem item)
    {
        await _db.CartItems.AddAsync(item);
    }

    public Task RemoveItemAsync(CartItem item)
    {
        _db.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _db.CartItems
            .Where(ci => ci.Cart.UserId == userId)
            .ToListAsync();
        _db.CartItems.RemoveRange(items);
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
