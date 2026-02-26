using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace Zubs.WebApi.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _service;

    public DoctorsController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var doctor = await _service.GetByIdAsync(id);
        return doctor is null ? NotFound() : Ok(doctor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DoctorCreateDto dto)
        => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, DoctorUpdateDto dto)
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
