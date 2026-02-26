using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Zubs.Application.Auth;
using Zubs.Application.Interfaces.Auth;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;

namespace Zubs.Application.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;
    private readonly JwtOptions _options;

    public RefreshTokenService(
        IRefreshTokenRepository repository,
        IOptions<JwtOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var tokenString = GenerateRandomToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = tokenString,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(refreshToken);

        return tokenString;
    }

    public async Task<Guid> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _repository.GetByTokenAsync(token);

        if (refreshToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (refreshToken.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token has been revoked");

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired");

        return refreshToken.UserId;
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _repository.GetByTokenAsync(token);

        if (refreshToken == null)
            throw new KeyNotFoundException("Refresh token not found");

        refreshToken.IsRevoked = true;
        await _repository.UpdateAsync(refreshToken);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _repository.GetByUserIdAsync(userId);

        foreach (var token in tokens.Where(t => !t.IsRevoked))
        {
            token.IsRevoked = true;
            await _repository.UpdateAsync(token);
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _repository.GetExpiredTokensAsync();

        foreach (var token in expiredTokens)
        {
            await _repository.DeleteAsync(token.Id);
        }
    }

    private static string GenerateRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}