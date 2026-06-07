using System.Data;
using Dapper;
using NoteApi.Data;
using NoteApi.Models;

namespace NoteApi.Repositories;

public class AuthRepository(ISqlConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        using var db = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT Id, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email";
        return await db.QueryFirstOrDefaultAsync<UserEntity>(sql, new { Email = email });
    }

    public async Task<int> CreateUserAsync(string email, string passwordHash)
    {
        using var db = await connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO Users (Email, PasswordHash, CreatedAt) OUTPUT INSERTED.Id VALUES (@Email, @PasswordHash, GETUTCDATE())";
        return await db.ExecuteScalarAsync<int>(sql, new { Email = email, PasswordHash = passwordHash });
    }

    public async Task SaveRefreshTokenAsync(string token)
    {
        using var db = await connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO UserRefreshTokens (Token, IsUsed) VALUES (@Token, 0)";
        await db.ExecuteAsync(sql, new { Token = token });
    }

    public async Task<UserRefreshTokenEntity?> GetRefreshTokenAsync(string token)
    {
        using var db = await connectionFactory.CreateConnectionAsync();
        return await db.QueryFirstOrDefaultAsync<UserRefreshTokenEntity>(
            "SELECT Id, Token, IsUsed FROM UserRefreshTokens WHERE Token = @Token", new { Token = token });
    }

    public async Task MarkTokenAsUsedAsync(string token)
    {
        using var db = await connectionFactory.CreateConnectionAsync();
        await db.ExecuteAsync("UPDATE UserRefreshTokens SET IsUsed = 1 WHERE Token = @Token", new { Token = token });
    }
}