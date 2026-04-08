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
    private readonly ICacheService _cache;

    private static string ByIdKey(Guid id) => $"services:{id}";
    private const string AllKey = "services:all";

    public ServiceService(IServiceRepository repo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<ServiceDto>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<IEnumerable<ServiceDto>>(AllKey);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<ServiceDto>>(await _repo.GetAllAsync());
        await _cache.SetAsync(AllKey, result, TimeSpan.FromMinutes(30));
        return result;
    }

    public async Task<ServiceDto?> GetByIdAsync(Guid id)
    {
        var key = ByIdKey(id);
        var cached = await _cache.GetAsync<ServiceDto>(key);
        if (cached is not null) return cached;

        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return null;

        var dto = _mapper.Map<ServiceDto>(entity);
        await _cache.SetAsync(key, dto, TimeSpan.FromMinutes(60));
        return dto;
    }

    public async Task<ServiceDto> CreateAsync(ServiceCreateDto dto)
    {
        if (await _repo.ExistsByCodeAsync(dto.Code))
            throw new AppException($"Service with code '{dto.Code}' already exists.");

        var entity = _mapper.Map<Service>(dto);
        entity.Id = Guid.NewGuid();
        await _repo.AddAsync(entity);

        await _cache.RemoveAsync(AllKey);

        return _mapper.Map<ServiceDto>(entity);
    }

    public async Task UpdateAsync(ServiceUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Service with id {dto.Id} not found.");

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
            throw new KeyNotFoundException($"Service with id {id} not found.");

        await _repo.DeleteAsync(id);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(id)),
            _cache.RemoveAsync(AllKey)
        );
    }
}