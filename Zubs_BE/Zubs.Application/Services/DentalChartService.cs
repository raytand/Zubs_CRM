using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class DentalChartService : IDentalChartService
{
    private readonly IDentalChartRepository _repo;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    private static string ByIdKey(Guid id) => $"dentalchart:{id}";
    private static string ByPatientKey(Guid patientId) => $"dentalchart:patient:{patientId}";

    public DentalChartService(IDentalChartRepository repo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<DentalChartDto>> GetByPatientAsync(Guid patientId)
    {
        var key = ByPatientKey(patientId);
        var cached = await _cache.GetAsync<IEnumerable<DentalChartDto>>(key);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<DentalChartDto>>(await _repo.GetByPatientAsync(patientId));
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(15));
        return result;
    }

    public async Task<DentalChartDto?> GetByIdAsync(Guid id)
    {
        var key = ByIdKey(id);
        var cached = await _cache.GetAsync<DentalChartDto>(key);
        if (cached is not null) return cached;

        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return null;

        var dto = _mapper.Map<DentalChartDto>(entity);
        await _cache.SetAsync(key, dto, TimeSpan.FromMinutes(15));
        return dto;
    }

    public async Task<DentalChartDto> CreateAsync(DentalChartCreateDto dto)
    {
        var entity = _mapper.Map<DentalChart>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);

        await _cache.RemoveAsync(ByPatientKey(entity.PatientId));

        return _mapper.Map<DentalChartDto>(entity);
    }

    public async Task UpdateAsync(DentalChartUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null) return;

        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(dto.Id)),
            _cache.RemoveAsync(ByPatientKey(entity.PatientId))
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;

        await _repo.DeleteAsync(id);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(id)),
            _cache.RemoveAsync(ByPatientKey(entity.PatientId))
        );
    }
}