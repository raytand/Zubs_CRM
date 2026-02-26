using Zubs.Application.DTOs;

namespace Zubs.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync();
    Task LogAsync(
            string entity,
            Guid entityId,
            string action
        );
}
