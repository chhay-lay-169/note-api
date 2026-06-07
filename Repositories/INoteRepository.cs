using NoteApi.Models;

namespace NoteApi.Repositories;

public interface INoteRepository
{
    Task<IEnumerable<NoteEntity>> GetByUserIdAsync(int userId, CancellationToken ct);
    Task<NoteEntity?> GetByIdAsync(int id, int userId, CancellationToken ct);
    Task<int> CreateAsync(NoteEntity note, CancellationToken ct); // Returns the newly generated database ID
    Task<bool> UpdateAsync(NoteEntity note, CancellationToken ct);
    Task<bool> DeleteAsync(int id, int userId, CancellationToken ct);
}