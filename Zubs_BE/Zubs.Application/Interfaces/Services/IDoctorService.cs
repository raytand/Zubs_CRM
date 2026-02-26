using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<DoctorDto?> GetByIdAsync(Guid id);
    Task<DoctorDto> CreateAsync(DoctorCreateDto dto);
    Task UpdateAsync(DoctorUpdateDto dto);
    Task DeleteAsync(Guid id);
}
