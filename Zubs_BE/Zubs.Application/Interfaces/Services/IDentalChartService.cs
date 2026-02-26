using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IDentalChartService
{
    Task<IEnumerable<DentalChartDto>> GetByPatientAsync(Guid patientId);
    Task<DentalChartDto?> GetByIdAsync(Guid id);
    Task<DentalChartDto> CreateAsync(DentalChartCreateDto dto);
    Task UpdateAsync(DentalChartUpdateDto dto);
    Task DeleteAsync(Guid id);
}
