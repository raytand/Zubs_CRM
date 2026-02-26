using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<PaymentDto> CreateAsync(PaymentCreateDto dto);
    Task UpdateAsync(PaymentUpdateDto dto);
    Task DeleteAsync(Guid id);
}
