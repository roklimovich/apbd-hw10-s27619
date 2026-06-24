using Microsoft.EntityFrameworkCore;
using UserPanelApp.Data;
using UserPanelApp.Models;

namespace UserPanelApp.Services;

/// <summary>
/// Handles password hashing (BCrypt, work factor 12) and user persistence.
/// Passwords are NEVER logged or returned to callers — only the hash is stored.
/// </summary>
public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registers a new user with the "User" role.
    /// Returns null if the email is already taken.
    /// </summary>
    public async Task<AppUser?> RegisterAsync(string email, string password)
    {
        var emailLower = email.Trim().ToLowerInvariant();

        if (await _context.AppUsers.AnyAsync(u => u.Email == emailLower))
            return null; // email taken

        // BCrypt generates a unique salt internally and embeds it in the hash string.
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var user = new AppUser
        {
            Email = emailLower,
            PasswordHash = hash,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Validates credentials. Returns the user on success, null on failure.
    /// Callers must NOT distinguish between "user not found" and "wrong password"
    /// in user-facing messages.
    /// </summary>
    public async Task<AppUser?> ValidateAsync(string email, string password)
    {
        var emailLower = email.Trim().ToLowerInvariant();
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == emailLower);

        if (user == null)
            return null;

        // BCrypt.Verify extracts the salt from the stored hash automatically.
        var valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return valid ? user : null;
    }
}
