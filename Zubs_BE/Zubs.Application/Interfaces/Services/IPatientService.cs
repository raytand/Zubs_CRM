using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto?> GetByIdAsync(Guid id);
    Task<PatientDto> CreateAsync(PatientCreateDto dto);
    Task UpdateAsync(PatientUpdateDto dto);
    Task DeleteAsync(Guid id);
}
