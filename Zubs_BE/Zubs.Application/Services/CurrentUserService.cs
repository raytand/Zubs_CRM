using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Zubs.Application.Interfaces.Helpers;

namespace Zubs.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid? UserId
    {
        get
        {
            var value = _http.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }
}
