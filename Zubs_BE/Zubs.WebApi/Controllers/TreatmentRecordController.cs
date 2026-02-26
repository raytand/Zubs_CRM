using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;

namespace Zubs.API.Controllers;

[ApiController]
[Route("api/treatment-records")]
public class TreatmentRecordController : ControllerBase
{
    private readonly ITreatmentRecordService _service;

    public TreatmentRecordController(ITreatmentRecordService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TreatmentRecordDto>> Get(Guid id)
    {
        var record = await _service.GetByIdAsync(id);
        return record == null ? NotFound() : Ok(record);
    }

    [HttpGet("by-appointment/{appointmentId:guid}")]
    public async Task<ActionResult<IEnumerable<TreatmentRecordDto>>> GetByAppointment(Guid appointmentId)
        => Ok(await _service.GetByAppointmentAsync(appointmentId));

    [HttpPost]
    public async Task<ActionResult<TreatmentRecordDto>> Create(TreatmentRecordCreateDto dto)
        => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TreatmentRecordDto dto)
    {
        if (id != dto.Id) return BadRequest();
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
