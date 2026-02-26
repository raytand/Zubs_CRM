using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class ServiceRepository : BaseRepository<Service>, IServiceRepository
{
    public ServiceRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbSet.AnyAsync(s => s.Code == code);
    }
}
