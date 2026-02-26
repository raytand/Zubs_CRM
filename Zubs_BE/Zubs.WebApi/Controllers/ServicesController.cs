using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;

namespace Zubs.WebApi.Controllers;

[Authorize(Roles = "Admin,Doctor")]
[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _service;

    public ServicesController(IServiceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var service = await _service.GetByIdAsync(id);
        return service is null ? NotFound() : Ok(service);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ServiceCreateDto dto)
        => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ServiceUpdateDto dto)
    { 
        if (id != dto.Id)
        {
            return BadRequest();
        }
        await _service.UpdateAsync(dto);
        return NoContent();
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
