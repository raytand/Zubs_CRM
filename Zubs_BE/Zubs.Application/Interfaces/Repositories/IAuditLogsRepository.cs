using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId);
}
