using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zubs.Application.DTOs;
using Zubs.Application.Interfaces.Auth;
using Zubs.Application.Interfaces.Services;
using Zubs.Domain.Entities;

namespace Zubs.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshToken;

    public AuthController(
        IUserService users,
        IJwtTokenService jwt,
        IRefreshTokenService refreshToken)
    {
        _users = users;
        _jwt = jwt;
        _refreshToken = refreshToken;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] UserCredentialsDto dto)
    {
        var user = await _users.LoginAsync(dto);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var accessToken = _jwt.GenerateToken(user);
        var refreshToken = await _refreshToken.GenerateRefreshTokenAsync(user.Id);

        return Ok(new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600 // 1 hour in seconds
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        await _users.RegisterAsync(dto);
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh([FromBody] RefreshRequestDto dto)
    {
        try
        {
            var userId = await _refreshToken.ValidateRefreshTokenAsync(dto.RefreshToken);

            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found");

            // Revoke old refresh token
            await _refreshToken.RevokeRefreshTokenAsync(dto.RefreshToken);

            var userDto = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
            // Generate new tokens
            var accessToken = _jwt.GenerateToken(userDto);
            var newRefreshToken = await _refreshToken.GenerateRefreshTokenAsync(user.Id);

            return Ok(new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
    {
        try
        {
            await _refreshToken.RevokeRefreshTokenAsync(dto.RefreshToken);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

        await _refreshToken.RevokeAllUserTokensAsync(userId);
        return Ok(new { message = "All tokens revoked successfully" });
    }
}


public class AuthResultDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
}

public class RefreshRequestDto
{
    public string RefreshToken { get; set; } = null!;
}