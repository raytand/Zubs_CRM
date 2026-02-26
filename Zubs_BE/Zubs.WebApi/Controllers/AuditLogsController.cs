using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace Zubs.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Doctor")]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(AuditLogCreateDto dto)
    {
        await _service.LogAsync(dto.Entity, dto.EntityId, dto.Action);
        return Ok();
    }
}
