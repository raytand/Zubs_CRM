namespace Zubs.Application.Interfaces.Auth;

public interface IRefreshTokenService
{
    /// <summary>
    /// Generate a new refresh token for a user
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(Guid userId);

    /// <summary>
    /// Validate refresh token and return user ID
    /// </summary>
    Task<Guid> ValidateRefreshTokenAsync(string token);

    /// <summary>
    /// Revoke a specific refresh token
    /// </summary>
    Task RevokeRefreshTokenAsync(string token);

    /// <summary>
    /// Revoke all refresh tokens for a user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId);

    /// <summary>
    /// Clean up expired tokens
    /// </summary>
    Task CleanupExpiredTokensAsync();
}