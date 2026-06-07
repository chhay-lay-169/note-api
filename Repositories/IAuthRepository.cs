using NoteApi.Models;

namespace NoteApi.Repositories;

public interface IAuthRepository
{
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<int> CreateUserAsync(string email, string passwordHash);
    Task SaveRefreshTokenAsync(string token);
    Task<UserRefreshTokenEntity?> GetRefreshTokenAsync(string token);
    Task MarkTokenAsUsedAsync(string token);
}