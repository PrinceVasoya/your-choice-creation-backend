using EcommerceCA.Application.Interfaces.Repositories;
using EcommerceCA.Domain.Entities;
using EcommerceCA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCA.Infrastructure.Repositories.Implementations.Payment;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;
    public PaymentRepository(ApplicationDbContext db) => _db = db;

    public async Task<Domain.Entities.Payment?> GetByOrderIdAsync(int orderId) =>
        await _db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task AddAsync(Domain.Entities.Payment payment) =>
        await _db.Payments.AddAsync(payment);

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
