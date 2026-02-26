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

    public DoctorService(IDoctorRepository repo,IUserRepository userRepo, IMapper mapper)
    {
        _repo = repo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<DoctorDto>>(await _repo.GetAllAsync());

    public async Task<DoctorDto?> GetByIdAsync(Guid id)
        => _mapper.Map<DoctorDto?>(await _repo.GetByIdAsync(id));

    public async Task<DoctorDto> CreateAsync(DoctorCreateDto dto)
    {
        var user = await _userRepo.GetByIdAsync(dto.UserId)
           ?? throw new KeyNotFoundException("User not found");

        var entity = _mapper.Map<Doctor>(dto);
        entity.Id = Guid.NewGuid();  
        entity.User = user;          
        entity.UserId = user.Id;     

        await _repo.AddAsync(entity);
        return _mapper.Map<DoctorDto>(entity);
    }

    public async Task UpdateAsync(DoctorUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity == null)
            throw new KeyNotFoundException($"Doctor with id {dto.Id} not found.");

        _mapper.Map(dto, entity);

        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        if(await _repo.GetByIdAsync(id) is null)
        {
            throw new KeyNotFoundException($"Doctor with id {id} not found.");
        }
        await _repo.DeleteAsync(id);
    }
}
