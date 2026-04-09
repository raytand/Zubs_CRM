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
    private readonly ICacheService _cache;

    private static string ByIdKey(Guid id) => $"treatmentrecords:{id}";
    private static string ByAppointmentKey(Guid appointmentId) => $"treatmentrecords:appointment:{appointmentId}";

    public TreatmentRecordService(ITreatmentRecordRepository repo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<TreatmentRecordDto?> GetByIdAsync(Guid id)
    {
        var key = ByIdKey(id);
        var cached = await _cache.GetAsync<TreatmentRecordDto>(key);
        if (cached is not null) return cached;

        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return null;

        var dto = _mapper.Map<TreatmentRecordDto>(entity);
        await _cache.SetAsync(key, dto, TimeSpan.FromMinutes(15));
        return dto;
    }

    public async Task<IEnumerable<TreatmentRecordDto>> GetByAppointmentAsync(Guid appointmentId)
    {
        var key = ByAppointmentKey(appointmentId);
        var cached = await _cache.GetAsync<IEnumerable<TreatmentRecordDto>>(key);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<TreatmentRecordDto>>(await _repo.GetByAppointmentAsync(appointmentId));
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(15));
        return result;
    }

    public async Task<TreatmentRecordDto> CreateAsync(TreatmentRecordCreateDto dto)
    {
        var entity = _mapper.Map<TreatmentRecord>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);

        await _cache.RemoveAsync(ByAppointmentKey(entity.AppointmentId));

        return _mapper.Map<TreatmentRecordDto>(entity);
    }

    public async Task UpdateAsync(TreatmentRecordDto dto)
    {
        var entity = _mapper.Map<TreatmentRecord>(dto);
        await _repo.UpdateAsync(entity);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(dto.Id)),
            _cache.RemoveAsync(ByAppointmentKey(dto.AppointmentId))
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;

        await _repo.DeleteAsync(id);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(id)),
            _cache.RemoveAsync(ByAppointmentKey(entity.AppointmentId))
        );
    }
}