using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;

namespace Zubs.WebApi.Controllers;

[Authorize(Roles = "Admin,Doctor,Secretary")]
[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
        => Ok(await _service.GetByDoctorAsync(doctorId));

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId)
        => Ok(await _service.GetByPatientAsync(patientId));

    [HttpPost]
    public async Task<IActionResult> Create(AppointmentCreateDto dto)
        => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AppointmentUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

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
