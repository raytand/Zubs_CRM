using Zubs.Application.DTOs;
using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task UpdateAsync(UserUpdateDto dto);
    Task DeleteAsync(Guid id);
    Task RegisterAsync(RegisterDto dto);
    Task<User?> LoginAsync(UserCredentialsDto dto);
}
