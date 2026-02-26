using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();
    Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(Guid doctorId);
    Task<IEnumerable<AppointmentDto>> GetByPatientAsync(Guid patientId);
    Task<AppointmentDto> CreateAsync(AppointmentCreateDto dto);
    Task UpdateAsync(AppointmentUpdateDto dto);
    Task DeleteAsync(Guid id);
}
