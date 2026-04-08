using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _repo;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    private static string ByPatientKey(Guid patientId) => $"medicalrecords:patient:{patientId}";

    public MedicalRecordService(IMedicalRecordRepository repo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetByPatientAsync(Guid patientId)
    {
        var key = ByPatientKey(patientId);
        var cached = await _cache.GetAsync<IEnumerable<MedicalRecordDto>>(key);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<MedicalRecordDto>>(await _repo.GetByPatientIdAsync(patientId));
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<MedicalRecordDto> CreateAsync(MedicalRecordCreateDto dto)
    {
        var entity = _mapper.Map<MedicalRecord>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(entity);

        await _cache.RemoveAsync(ByPatientKey(entity.PatientId));

        return _mapper.Map<MedicalRecordDto>(entity);
    }

    public async Task UpdateAsync(MedicalRecordUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Medical Record with ID {dto.Id} not found.");

        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);

        await _cache.RemoveAsync(ByPatientKey(entity.PatientId));
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;

        await _repo.DeleteAsync(id);

        await _cache.RemoveAsync(ByPatientKey(entity.PatientId));
    }
}