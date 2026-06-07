namespace NoteApi.Models;

// The exact database table representation
public record NoteEntity
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateNoteDto(string Title, string Content);
public record UpdateNoteDto(string Title, string Content);
public record NoteListResponseDto(int Id, string Title, DateTime CreatedAt);
public record NoteDetailResponseDto(int Id, int UserId, string Title, string Content, DateTime CreatedAt);