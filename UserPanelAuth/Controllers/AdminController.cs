using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserPanelApp.Data;

namespace UserPanelApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _context.AppUsers
            .AsNoTracking()
            .Select(u => new { u.Id, u.Email, u.Role, u.CreatedAt, NoteCount = u.Notes.Count })
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return View(users);
    }
}
