using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Zubs.Application.Auth;
using Zubs.Application.Interfaces.Auth;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Services;
using Zubs.Domain.Entities;
using Zubs.Domain.Enums;
using Zubs.Infrastructure.Auth;
using Xunit;

namespace Zubs.Tests;

public class AuthTests
{
    [Fact]
    public void JwtTokenService_Generates_Jwt_String()
    {
        var options = Options.Create(new JwtOptions
        {
            Key = "supersecretkey1234567890abcdef12",  // 32 chars = 256 bits (minimum for HS256)
            Issuer = "test",
            Audience = "test",
            ExpireMinutes = 60
        });

        var svc = new JwtTokenService(options);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Role = UserRole.Admin
        };

        var token = svc.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Contains('.', token); // JWT has dot separators
    }

    [Fact]
    public async Task RefreshTokenService_Generate_Validate_Revoke_Workflow()
    {
        var repo = new InMemoryRefreshTokenRepository();
        var options = Options.Create(new JwtOptions { RefreshDays = 1 });
        var svc = new RefreshTokenService(repo, options);

        var userId = Guid.NewGuid();
        var token = await svc.GenerateRefreshTokenAsync(userId);

        Assert.False(string.IsNullOrWhiteSpace(token));

        var validatedUserId = await svc.ValidateRefreshTokenAsync(token);
        Assert.Equal(userId, validatedUserId);

        await svc.RevokeRefreshTokenAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.ValidateRefreshTokenAsync(token));
    }

    [Fact]
    public async Task UserService_Register_Login_Update_Delete()
    {
        var repo = new InMemoryUserRepository();

        // Configure AutoMapper with proper mappings
        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            // User -> UserDto: Role enum needs to convert to string
            cfg.CreateMap<User, Zubs.Application.DTOs.UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // RegisterDto -> User (ignore properties not provided by the DTO)
            cfg.CreateMap<Zubs.Application.DTOs.RegisterDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.Doctors, opt => opt.Ignore())
                .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());

            // UserUpdateDto -> User (ignore properties not provided by the DTO)
            cfg.CreateMap<Zubs.Application.DTOs.UserUpdateDto, User>()
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.Doctors, opt => opt.Ignore())
                .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());
        });

        // Validate configuration - catches mapping errors immediately
        config.AssertConfigurationIsValid();

        var mapper = config.CreateMapper();
        var svc = new UserService(repo, mapper);

        var registerDto = new Zubs.Application.DTOs.RegisterDto
        {
            Username = "john",
            Email = "john@example.com",
            Password = "password123"
        };

        await svc.RegisterAsync(registerDto);

        var userDto = await svc.GetByUsernameAsync("john");
        Assert.NotNull(userDto);
        Assert.Equal("john", userDto!.Username);

        var loginUser = await svc.LoginAsync(new Zubs.Application.DTOs.UserCredentialsDto
        {
            Username = "john",
            Password = "password123"
        });

        Assert.NotNull(loginUser);

        var updateDto = new Zubs.Application.DTOs.UserUpdateDto
        {
            Id = loginUser!.Id,
            Email = "john2@example.com",
            Role = UserRole.Doctor
        };

        await svc.UpdateAsync(updateDto);

        var updated = await repo.GetByIdAsync(updateDto.Id);
        Assert.Equal("john2@example.com", updated!.Email);
        Assert.Equal(UserRole.Doctor, updated.Role);

        await svc.DeleteAsync(updateDto.Id);

        await Assert.ThrowsAsync<Exception>(async () => await svc.DeleteAsync(updateDto.Id));
    }

    // Simple in-memory implementations for repositories used in tests
    private class InMemoryRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ConcurrentDictionary<Guid, RefreshToken> _store = new();

        public Task AddAsync(RefreshToken refreshToken)
        {
            _store[refreshToken.Id] = refreshToken;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _store.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
        {
            return Task.FromResult(_store.Values.Where(t => t.UserId == userId).AsEnumerable());
        }

        public Task<RefreshToken?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var t);
            return Task.FromResult(t);
        }

        public Task<RefreshToken?> GetByTokenAsync(string token)
        {
            var t = _store.Values.FirstOrDefault(x => x.Token == token);
            return Task.FromResult(t);
        }

        public Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync()
        {
            return Task.FromResult(_store.Values.Where(x => x.ExpiresAt < DateTime.UtcNow || x.IsRevoked).AsEnumerable());
        }

        public Task UpdateAsync(RefreshToken refreshToken)
        {
            _store[refreshToken.Id] = refreshToken;
            return Task.CompletedTask;
        }
    }

    private class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<Guid, User> _store = new();

        public Task AddAsync(User entity)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _store.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult(_store.Values.AsEnumerable());
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var u);
            return Task.FromResult(u);
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            var u = _store.Values.FirstOrDefault(x => x.Username == username);
            return Task.FromResult(u);
        }

        public Task SaveChangesAsync() => Task.CompletedTask;

        public Task UpdateAsync(User entity)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }
    }
}