using System.Security.Claims;

namespace NoteApi.Services;

public interface ITokenService
{
    string GenerateAccessToken(int userId, string username);
    string GenerateRefreshToken(int userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    ClaimsPrincipal? ValidateRefreshToken(string token);
}