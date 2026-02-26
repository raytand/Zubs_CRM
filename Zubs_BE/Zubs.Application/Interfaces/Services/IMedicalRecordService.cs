using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IMedicalRecordService
{
    Task<IEnumerable<MedicalRecordDto>> GetByPatientAsync(Guid patientId);
    Task<MedicalRecordDto> CreateAsync(MedicalRecordCreateDto dto);
    Task UpdateAsync(MedicalRecordUpdateDto dto);
    Task DeleteAsync(Guid id);

}
