using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface ITreatmentRecordService
{
    Task<TreatmentRecordDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TreatmentRecordDto>> GetByAppointmentAsync(Guid appointmentId);
    Task<TreatmentRecordDto> CreateAsync(TreatmentRecordCreateDto dto);
    Task UpdateAsync(TreatmentRecordDto dto);
    Task DeleteAsync(Guid id);
}
