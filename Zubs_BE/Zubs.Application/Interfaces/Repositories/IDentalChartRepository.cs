using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Repositories
{
    public interface IDentalChartRepository : IRepository<DentalChart>
    {
        Task<IEnumerable<DentalChart>> GetByPatientAsync(Guid patientId);
    }
}
