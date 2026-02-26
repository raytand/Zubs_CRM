using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;

namespace Zubs.WebApi.Controllers;
[Authorize(Roles = "Admin,Doctor,Secretary")]
[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientsController(IPatientService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var patient = await _service.GetByIdAsync(id);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PatientCreateDto dto)
        => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PatientUpdateDto dto)
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
