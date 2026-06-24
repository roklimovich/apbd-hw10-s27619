using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserPanelApp.DTOs;
using UserPanelApp.Services;

namespace UserPanelApp.Controllers;


[Authorize]
public class DashboardController : Controller
{
    private readonly NoteService _noteService;

    public DashboardController(NoteService noteService)
    {
        _noteService = noteService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var notes = await _noteService.GetUserNotesAsync(CurrentUserId);
        return View(notes);
    }

    [HttpGet]
    public IActionResult AddNote()
    {
        return View(new AddNoteViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(AddNoteViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _noteService.AddNoteAsync(CurrentUserId, model.Title, model.Content);
        TempData["Success"] = "Note added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(int noteId)
    {
        // NoteService enforces ownership — a user can only delete their own notes
        await _noteService.DeleteNoteAsync(noteId, CurrentUserId);
        TempData["Success"] = "Note deleted.";
        return RedirectToAction(nameof(Index));
    }
}
