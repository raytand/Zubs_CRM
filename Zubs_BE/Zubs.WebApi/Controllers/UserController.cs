using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Services;

namespace Zubs.API.Controllers;
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> Get(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<UserDto?>> GetByUsername(string username)
    {
        var user = await _service.GetByUsernameAsync(username);
        return user == null ? NotFound() : Ok(user);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UserUpdateDto dto)
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
