using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class DentalChartRepository
    : BaseRepository<DentalChart>, IDentalChartRepository
{
    public DentalChartRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DentalChart>> GetByPatientAsync(Guid patientId)
    {
        return await _dbSet
            .Where(dc => dc.PatientId == patientId)
            .ToListAsync();
    }
}
