using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories
{
    internal class TreatmentRecordRepository : BaseRepository<TreatmentRecord>, ITreatmentRecordRepository
    {
        public TreatmentRecordRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<TreatmentRecord>> GetByAppointmentAsync(Guid appointmentId)
    => await _context.TreatmentRecords
        .AsNoTracking()
        .Where(x => x.AppointmentId == appointmentId)
        .OrderBy(x => x.PerformedAt)
        .ToListAsync();
    }
}
