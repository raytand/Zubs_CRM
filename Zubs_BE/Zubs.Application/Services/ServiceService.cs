using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Exceptions;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repo;
    private readonly IMapper _mapper;

    public ServiceService(IServiceRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<ServiceDto>>(await _repo.GetAllAsync());

    public async Task<ServiceDto?> GetByIdAsync(Guid id)
        => _mapper.Map<ServiceDto?>(await _repo.GetByIdAsync(id));

    public async Task<ServiceDto> CreateAsync(ServiceCreateDto dto)
    {
        if (await _repo.ExistsByCodeAsync(dto.Code))
        {
            throw new AppException($"Service with code '{dto.Code}' already exists.");
        }

        var entity = _mapper.Map<Service>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);
        return _mapper.Map<ServiceDto>(entity);
    }

    public async Task UpdateAsync(ServiceUpdateDto dto)
    {

        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity == null)
            throw new KeyNotFoundException($"Service with id {dto.Id} not found.");

        _mapper.Map(dto, entity); 

        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (await _repo.GetByIdAsync(id) is null)
        {
            throw new KeyNotFoundException($"Service with id {id} not found.");
        }
        await _repo.DeleteAsync(id);
    }
}
