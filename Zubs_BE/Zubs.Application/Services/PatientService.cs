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

    public PatientService(IPatientRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<PatientDto>>(await _repo.GetAllAsync());

    public async Task<PatientDto?> GetByIdAsync(Guid id)
        => _mapper.Map<PatientDto?>(await _repo.GetByIdAsync(id));

    public async Task<PatientDto> CreateAsync(PatientCreateDto dto)
    {
        var entity = _mapper.Map<Patient>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(entity);
        return _mapper.Map<PatientDto>(entity);
    }
    public async Task UpdateAsync(PatientUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Patient with ID {dto.Id} not found.");
        }
        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);

    }
    public async Task DeleteAsync(Guid id)
    {
        if(await _repo.GetByIdAsync(id) is null)
        {
            throw new KeyNotFoundException($"Patient with ID {id} not found.");
        }
        await _repo.DeleteAsync(id);
    }
}
