using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly IMapper _mapper;

    public PaymentService(IPaymentRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<PaymentDto>>(await _repo.GetAllAsync());

    public async Task<PaymentDto?> GetByIdAsync(Guid id)
        => _mapper.Map<PaymentDto?>(await _repo.GetByIdAsync(id));

    public async Task<PaymentDto> CreateAsync(PaymentCreateDto dto)
    {
        var entity = _mapper.Map<Payment>(dto);
        entity.Id = Guid.NewGuid();

        await _repo.AddAsync(entity);

        return _mapper.Map<PaymentDto>(entity);
    }

    public async Task UpdateAsync(PaymentUpdateDto dto)
    {
        if (dto.Id == Guid.Empty)
        {
            throw new ArgumentException("Payment ID cannot be empty.");
        } 
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Payment with ID {dto.Id} not found.");
        }
        _mapper.Map(dto, entity);

        await _repo.UpdateAsync(entity);
    }
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity != null)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
