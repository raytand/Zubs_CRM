using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public override async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Patient)
            .Include(p => p.Appointment)
            .AsNoTracking()
            .ToListAsync();
    }
}
