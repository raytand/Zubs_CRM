using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
