using Microsoft.EntityFrameworkCore;
using UserPanelApp.Data;
using UserPanelApp.Models;

namespace UserPanelApp.Services;

public class NoteService
{
    private readonly AppDbContext _context;

    public NoteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserNote>> GetUserNotesAsync(int userId)
    {
        return await _context.UserNotes
            .AsNoTracking()
            .Where(n => n.AppUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task AddNoteAsync(int userId, string title, string content)
    {
        _context.UserNotes.Add(new UserNote
        {
            AppUserId = userId,
            Title = title.Trim(),
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteNoteAsync(int noteId, int userId)
    {
        var note = await _context.UserNotes.FirstOrDefaultAsync(n => n.Id == noteId && n.AppUserId == userId);
        if (note == null) return false;
        _context.UserNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }
}
