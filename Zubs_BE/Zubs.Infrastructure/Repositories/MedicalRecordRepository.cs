using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class MedicalRecordRepository : BaseRepository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId)
    {
        return await _dbSet
            .Where(m => m.PatientId == patientId)
            .AsNoTracking()
            .ToListAsync();
    }
}
