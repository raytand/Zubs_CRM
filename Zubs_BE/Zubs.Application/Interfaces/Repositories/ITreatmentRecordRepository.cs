using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Repositories
{
    public interface ITreatmentRecordRepository : IRepository<TreatmentRecord>
    {
        Task<IEnumerable<TreatmentRecord>> GetByAppointmentAsync(Guid appointmentId);
    }
}
