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

    public DentalChartService(IDentalChartRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DentalChartDto>> GetByPatientAsync(Guid patientId)
        => _mapper.Map<IEnumerable<DentalChartDto>>(
            await _repo.GetByPatientAsync(patientId));

    public async Task<DentalChartDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<DentalChartDto>(entity);
    }

    public async Task<DentalChartDto> CreateAsync(DentalChartCreateDto dto)
    {
        var entity = _mapper.Map<DentalChart>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);
        return _mapper.Map<DentalChartDto>(entity);
    }

    public async Task UpdateAsync(DentalChartUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity == null) return;

        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);
}
