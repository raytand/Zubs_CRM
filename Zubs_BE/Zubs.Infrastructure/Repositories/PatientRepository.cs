using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class PatientRepository : BaseRepository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context) { }

    public override async Task<Patient?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Appointments)
            .Include(p => p.Payments)
            .Include(p => p.MedicalRecords)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
