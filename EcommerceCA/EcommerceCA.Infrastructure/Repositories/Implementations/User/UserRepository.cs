using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.User;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Address>> GetAddressesAsync(string userId) =>
        await _db.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<Address?> GetAddressAsync(int id, string userId) =>
        await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

    public async Task AddAddressAsync(Address address) =>
        await _db.Addresses.AddAsync(address);

    public Task RemoveAddressAsync(Address address)
    {
        _db.Addresses.Remove(address);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
