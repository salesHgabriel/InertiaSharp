using Microsoft.AspNetCore.Identity;

namespace InertiaSharp.Sample.Models;

/// <summary>
/// Extended Identity user with extra profile fields.
/// </summary>
public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string? Bio      { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();
    
    public string? AvatarPathFile { get; set; }
    
    public bool HasAvatar => AvatarPathFile is not null;
}
