using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Auth;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _db;
    public RefreshTokenRepository(ApplicationDbContext db) => _db = db;

    public async Task<RefreshToken?> GetValidAsync(string token) =>
        await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.Token == token &&
                !r.IsRevoked &&
                r.ExpiresAt > DateTime.UtcNow);

    public async Task AddAsync(RefreshToken token)
    {
        await _db.RefreshTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
