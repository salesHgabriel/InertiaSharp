using InertiaSharp.Extensions;
using InertiaSharp.Sample.Models;
using InertiaSharp.Sample.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InertiaSharp.Sample.Controllers;

[Authorize]
[Route("dashboard")]
public class DashboardController : Controller
{
    private readonly UserManager<AppUser> _users;

    public DashboardController(UserManager<AppUser> users) => _users = users;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user  = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();
        var roles = await _users.GetRolesAsync(user);

        return this.Inertia("Dashboard", new
        {
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.CreatedAt,
                roles,
            },
            permissions = new
            {
                canEditContent = roles.Contains(Roles.Admin) || roles.Contains(Roles.Editor),
                canManageUsers = roles.Contains(Roles.Admin),
                canViewReports = true, // all authenticated users
            },
            stats = new
            {
                // Replace with real DB queries
                totalUsers    = 42,
                activeToday   = 7,
                pendingTasks  = 3,
            }
        });
    }
}
