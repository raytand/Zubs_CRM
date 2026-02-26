using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class DoctorRepository : BaseRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(AppDbContext context) : base(context) { }

    public override async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _dbSet
            .Include(d => d.User)
            .AsNoTracking()
            .ToListAsync();
    }
}
