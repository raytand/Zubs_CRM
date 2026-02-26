using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class TreatmentRecordService : ITreatmentRecordService
{
    private readonly ITreatmentRecordRepository _repo;
    private readonly IMapper _mapper;

    public TreatmentRecordService(
        ITreatmentRecordRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<TreatmentRecordDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<TreatmentRecordDto>(entity);
    }

    public async Task<IEnumerable<TreatmentRecordDto>> GetByAppointmentAsync(Guid appointmentId)
        => _mapper.Map<IEnumerable<TreatmentRecordDto>>(
            await _repo.GetByAppointmentAsync(appointmentId));

    public async Task<TreatmentRecordDto> CreateAsync(TreatmentRecordCreateDto dto)
    {
        var entity = _mapper.Map<TreatmentRecord>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);
        return _mapper.Map<TreatmentRecordDto>(entity);
    }

    public async Task UpdateAsync(TreatmentRecordDto dto)
    {
        var entity = _mapper.Map<TreatmentRecord>(dto);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);
}
