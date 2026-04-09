using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    private static string ByIdKey(Guid id) => $"patients:{id}";
    private const string AllKey = "patients:all";

    public PatientService(IPatientRepository repo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<IEnumerable<PatientDto>>(AllKey);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<PatientDto>>(await _repo.GetAllAsync());
        await _cache.SetAsync(AllKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<PatientDto?> GetByIdAsync(Guid id)
    {
        var key = ByIdKey(id);
        var cached = await _cache.GetAsync<PatientDto>(key);
        if (cached is not null) return cached;

        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return null;

        var dto = _mapper.Map<PatientDto>(entity);
        await _cache.SetAsync(key, dto, TimeSpan.FromMinutes(30));
        return dto;
    }

    public async Task<PatientDto> CreateAsync(PatientCreateDto dto)
    {
        var entity = _mapper.Map<Patient>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(entity);

        await _cache.RemoveAsync(AllKey);

        return _mapper.Map<PatientDto>(entity);
    }

    public async Task UpdateAsync(PatientUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Patient with ID {dto.Id} not found.");

        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(dto.Id)),
            _cache.RemoveAsync(AllKey)
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        if (await _repo.GetByIdAsync(id) is null)
            throw new KeyNotFoundException($"Patient with ID {id} not found.");

        await _repo.DeleteAsync(id);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(id)),
            _cache.RemoveAsync(AllKey)
        );
    }
}