using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;

namespace Zubs.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Doctor")]
[Route("api/dental-charts")]
public class DentalChartController : ControllerBase
{
    private readonly IDentalChartService _service;

    public DentalChartController(IDentalChartService service)
    {
        _service = service;
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(Guid patientId)
        => Ok(await _service.GetByPatientAsync(patientId));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DentalChartCreateDto dto)
        => Ok(await _service.CreateAsync(dto));

    [HttpPut]
    public async Task<IActionResult> Update(DentalChartUpdateDto dto)
    {
        await _service.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
