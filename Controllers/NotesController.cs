using Microsoft.AspNetCore.Mvc;
using NoteApi.Models;
using NoteApi.Repositories;

namespace NoteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteRepository _repository;
    private static readonly int MockUserId = 1; // Simplistic integer user mocking representation

    public NotesController(INoteRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoteListResponseDto>>> GetNotes(CancellationToken ct)
    {
        var notes = await _repository.GetByUserIdAsync(MockUserId, ct);
        var response = notes.Select(n => new NoteListResponseDto(n.Id, n.Title, n.CreatedAt));
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = nameof(GetNoteById))]
    public async Task<ActionResult<NoteDetailResponseDto>> GetNoteById(int id, CancellationToken ct)
    {
        var note = await _repository.GetByIdAsync(id, MockUserId, ct);
        if (note is null) return NotFound();

        var response = new NoteDetailResponseDto(note.Id, note.UserId, note.Title, note.Content, note.CreatedAt);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<NoteDetailResponseDto>> CreateNote([FromBody] CreateNoteDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title is required.");

        var note = new NoteEntity
        {
            UserId = MockUserId,
            Title = dto.Title,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        int newId = await _repository.CreateAsync(note, ct);
        
        var response = new NoteDetailResponseDto(newId, note.UserId, note.Title, note.Content, note.CreatedAt);
        return CreatedAtRoute(nameof(GetNoteById), new { id = newId }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title is required.");

        var existingNote = await _repository.GetByIdAsync(id, MockUserId, ct);
        if (existingNote is null) return NotFound();

        var updatedNote = existingNote with { Title = dto.Title, Content = dto.Content, UserId = MockUserId, UpdatedAt = DateTime.UtcNow };
        await _repository.UpdateAsync(updatedNote, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNote(int id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(id, MockUserId, ct);
        return deleted ? NoContent() : NotFound();
    }
}