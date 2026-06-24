namespace UserPanelApp.Models;

public class UserNote
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual AppUser User { get; set; } = null!;
}
