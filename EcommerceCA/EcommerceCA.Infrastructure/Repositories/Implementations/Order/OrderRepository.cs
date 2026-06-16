using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Domain.Enums;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Order;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderRepository(ApplicationDbContext db) => _db = db;

    public async Task<Domain.Entities.Order?> GetByIdAsync(int id) =>
        await _db.Orders.FindAsync(id);

    public async Task<IEnumerable<Domain.Entities.Order>> GetAllAsync() =>
        await _db.Orders.ToListAsync();

    public async Task<Domain.Entities.Order> AddAsync(Domain.Entities.Order entity)
    {
        await _db.Orders.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Domain.Entities.Order entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Order entity)
    {
        _db.Orders.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();

    public async Task<Domain.Entities.Order?> GetWithDetailsAsync(int id, string? userId = null)
    {
        var query = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .Where(o => o.Id == id);

        if (userId != null) query = query.Where(o => o.UserId == userId);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<(List<Domain.Entities.Order> Items, int Total)> GetUserOrdersPagedAsync(
        string userId, int page, int pageSize)
    {
        var query = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<(List<Domain.Entities.Order> Items, int Total)> GetAllOrdersPagedAsync(
        int page, int pageSize, OrderStatus? status)
    {
        var query = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        query = query.OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }
}
