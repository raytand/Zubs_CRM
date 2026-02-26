using AutoMapper;
using Microsoft.AspNetCore.Http;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Helpers;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public AuditLogService(IAuditLogRepository repo, ICurrentUserService currentUser, IMapper mapper)
    {
        _repo = repo;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<AuditLogDto>>(await _repo.GetAllAsync());

    public async Task LogAsync(string entity,Guid entityId,string action)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            Entity = entity,
            EntityId = entityId,
            Action = action,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = _currentUser.UserId ?? Guid.Empty
        };
        await _repo.AddAsync(log);
    }
}
