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

    public MedicalRecordService(IMedicalRecordRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetByPatientAsync(Guid patientId)
        => _mapper.Map<IEnumerable<MedicalRecordDto>>(
            await _repo.GetByPatientIdAsync(patientId));

    public async Task<MedicalRecordDto> CreateAsync(MedicalRecordCreateDto dto)
    {
        var entity = _mapper.Map<MedicalRecord>(dto);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        await _repo.AddAsync(entity);
        return _mapper.Map<MedicalRecordDto>(entity);
    }
    public async Task UpdateAsync(MedicalRecordUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity == null)
            throw new KeyNotFoundException($"Medical Record with ID {dto.Id} not found.");

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
