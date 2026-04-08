using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    private static string ByIdKey(Guid id) => $"doctors:{id}";
    private const string AllKey = "doctors:all";

    public DoctorService(IDoctorRepository repo, IUserRepository userRepo, IMapper mapper, ICacheService cache)
    {
        _repo = repo;
        _userRepo = userRepo;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<IEnumerable<DoctorDto>>(AllKey);
        if (cached is not null) return cached;

        var result = _mapper.Map<IEnumerable<DoctorDto>>(await _repo.GetAllAsync());
        await _cache.SetAsync(AllKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<DoctorDto?> GetByIdAsync(Guid id)
    {
        var key = ByIdKey(id);
        var cached = await _cache.GetAsync<DoctorDto>(key);
        if (cached is not null) return cached;

        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return null;

        var dto = _mapper.Map<DoctorDto>(entity);
        await _cache.SetAsync(key, dto, TimeSpan.FromMinutes(30));
        return dto;
    }

    public async Task<DoctorDto> CreateAsync(DoctorCreateDto dto)
    {
        var user = await _userRepo.GetByIdAsync(dto.UserId)
            ?? throw new KeyNotFoundException("User not found");

        var entity = _mapper.Map<Doctor>(dto);
        entity.Id = Guid.NewGuid();
        entity.User = user;
        entity.UserId = user.Id;
        await _repo.AddAsync(entity);

        await _cache.RemoveAsync(AllKey);

        return _mapper.Map<DoctorDto>(entity);
    }

    public async Task UpdateAsync(DoctorUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Doctor with id {dto.Id} not found.");

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
            throw new KeyNotFoundException($"Doctor with id {id} not found.");

        await _repo.DeleteAsync(id);

        await Task.WhenAll(
            _cache.RemoveAsync(ByIdKey(id)),
            _cache.RemoveAsync(AllKey)
        );
    }
}   