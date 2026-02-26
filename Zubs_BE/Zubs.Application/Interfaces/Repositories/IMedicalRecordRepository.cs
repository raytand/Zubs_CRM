using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Repositories
{
    public interface IMedicalRecordRepository : IRepository<MedicalRecord>
    {
        Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId);
    }
}
