using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using NoteApi.Models;
using NoteApi.Repositories;
using NoteApi.Services;

namespace NoteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthRepository authRepository, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var existingUser = await authRepository.GetByEmailAsync(dto.Email);
        if (existingUser is not null) return BadRequest("Email is already registered.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await authRepository.CreateUserAsync(dto.Email, passwordHash);
        return Ok(new { message = "Registration successful." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await authRepository.GetByEmailAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);

        await authRepository.SaveRefreshTokenAsync(refreshToken);

        return Ok(new AuthResponseDto(accessToken, refreshToken, user.Email));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var refreshPrincipal = tokenService.ValidateRefreshToken(dto.RefreshToken);
        if (refreshPrincipal is null) return Unauthorized("Refresh token is invalid or expired.");

        var userIdStr = refreshPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return BadRequest("Invalid token claims.");

        var dbRefreshToken = await authRepository.GetRefreshTokenAsync(dto.RefreshToken);
        if (dbRefreshToken is null || dbRefreshToken.IsUsed)
        {
            return Unauthorized("Token missing or has already been used.");
        }

        var accessPrincipal = tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        
        // CRITICAL: Pull ClaimTypes.Email here instead of Identity.Name
        var email = accessPrincipal?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return BadRequest("Invalid access token context.");

        await authRepository.MarkTokenAsUsedAsync(dto.RefreshToken);

        var newAccessToken = tokenService.GenerateAccessToken(userId, email);
        var newRefreshToken = tokenService.GenerateRefreshToken(userId);

        await authRepository.SaveRefreshTokenAsync(newRefreshToken);

        return Ok(new AuthResponseDto(newAccessToken, newRefreshToken, email));
    }
}