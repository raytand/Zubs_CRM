using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IServiceService
{
    Task<IEnumerable<ServiceDto>> GetAllAsync();
    Task<ServiceDto?> GetByIdAsync(Guid id);
    Task<ServiceDto> CreateAsync(ServiceCreateDto dto);
    Task UpdateAsync(ServiceUpdateDto dto);
    Task DeleteAsync(Guid id);
}
