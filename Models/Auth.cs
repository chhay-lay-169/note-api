using System.ComponentModel.DataAnnotations;

namespace NoteApi.Models;

public record UserEntity(int Id, string Email, string PasswordHash, DateTime CreatedAt);
public record UserRefreshTokenEntity(int Id, string Token, bool IsUsed);

public record RegisterDto(
    [Required][EmailAddress(ErrorMessage = "Invalid email format.")] string Email, 
    [Required][MinLength(6, ErrorMessage = "Password must be at least 6 characters.")] string Password
);

public record LoginDto(
    [Required][EmailAddress] string Email, 
    [Required] string Password
);

public record RefreshTokenRequestDto([Required] string AccessToken, [Required] string RefreshToken);

public record AuthResponseDto(string AccessToken, string RefreshToken, string Email);