using AutoMapper;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;
using Zubs.Domain.Enums;

namespace Zubs.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<UserDto>>(await _repo.GetAllAsync());

    public async Task<UserDto?> GetByIdAsync(Guid id)
        => _mapper.Map<UserDto?>(await _repo.GetByIdAsync(id));

    public async Task<UserDto?> GetByUsernameAsync(string username)
        => _mapper.Map<UserDto?>(await _repo.GetByUsernameAsync(username));

    public async Task UpdateAsync(UserUpdateDto dto)
    {
        var user = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException();

        user.Email = dto.Email;
        user.Role = dto.Role;

        await _repo.UpdateAsync(user);
    }
    public async Task DeleteAsync(Guid id)
    {
        if (await _repo.GetByIdAsync(id) == null)
            throw new Exception("User not found");
        await _repo.DeleteAsync(id);
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        if (await _repo.GetByUsernameAsync(dto.Username) != null)
            throw new Exception("Username already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Secretary,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(user);
    }
    public async Task<User?> LoginAsync(UserCredentialsDto dto)
    {
        var user = await _repo.GetByUsernameAsync(dto.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return user;
    }

}

