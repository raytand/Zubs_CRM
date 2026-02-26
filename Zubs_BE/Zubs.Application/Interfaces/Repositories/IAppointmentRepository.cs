using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId);
    }
}
