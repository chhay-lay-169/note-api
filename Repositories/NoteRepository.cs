using System.Data;
using Dapper;
using NoteApi.Data;
using NoteApi.Models;

namespace NoteApi.Repositories;

public class NoteRepository(ISqlConnectionFactory connectionFactory) : INoteRepository
{
    public async Task<IEnumerable<NoteEntity>> GetByUserIdAsync(int userId, CancellationToken ct)
    {
        using var db = await connectionFactory.CreateConnectionAsync(ct);
        const string sql = "SELECT Id, Title, CreatedAt FROM Notes WHERE UserId = @UserId ORDER BY CreatedAt DESC";
        return await db.QueryAsync<NoteEntity>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }

    public async Task<NoteEntity?> GetByIdAsync(int id, int userId, CancellationToken ct)
    {
        using var db = await connectionFactory.CreateConnectionAsync(ct);
        const string sql = "SELECT Id, Title, Content, CreatedAt, UpdatedAt FROM Notes WHERE Id = @Id AND UserId = @UserId";
        return await db.QueryFirstOrDefaultAsync<NoteEntity>(new CommandDefinition(sql, new { Id = id, UserId = userId }, cancellationToken: ct));
    }

    public async Task<int> CreateAsync(NoteEntity note, CancellationToken ct)
    {
        using var db = await connectionFactory.CreateConnectionAsync(ct);
        const string sql = """
            INSERT INTO Notes (UserId, Title, Content, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Title, @Content, @CreatedAt, @UpdatedAt);
            """;
        
        return await db.ExecuteScalarAsync<int>(new CommandDefinition(sql, note, cancellationToken: ct));
    }

    public async Task<bool> UpdateAsync(NoteEntity note, CancellationToken ct)
    {
        using var db = await connectionFactory.CreateConnectionAsync(ct);
        const string sql = """
            UPDATE Notes 
            SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt 
            WHERE Id = @Id AND UserId = @UserId;
            """;
        var rows = await db.ExecuteAsync(new CommandDefinition(sql, note, cancellationToken: ct));
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, int userId, CancellationToken ct)
    {
        using var db = await connectionFactory.CreateConnectionAsync(ct);
        const string sql = "DELETE FROM Notes WHERE Id = @Id AND UserId = @UserId";
        var rows = await db.ExecuteAsync(new CommandDefinition(sql, new { Id = id, UserId = userId }, cancellationToken: ct));
        return rows > 0;
    }
}