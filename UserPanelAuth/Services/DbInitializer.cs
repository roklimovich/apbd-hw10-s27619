using UserPanelApp.Data;
using UserPanelApp.Models;

namespace UserPanelApp.Services;

public static class DbInitializer
{
    /// <summary>
    /// Creates the database if it does not exist and seeds a default Admin account.
    /// The admin password is read from configuration (key "AdminSeed:Password").
    /// Falls back to a local demo value only — never commit real passwords.
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create schema (EnsureCreated is fine here; no migrations needed for this task)
        context.Database.EnsureCreated();

        if (context.AppUsers.Any(u => u.Role == "Admin"))
            return; // already seeded

        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin@1234!";
        var adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 12);

        context.AppUsers.Add(new AppUser
        {
            Email = "admin@example.com",
            PasswordHash = adminHash,
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}
