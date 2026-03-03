using InertiaSharp.Extensions;
using InertiaSharp.Sample.Models;
using InertiaSharp.Sample.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InertiaSharp.Sample.Controllers;

[Authorize(Policy = Policies.CanManageUsers)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _users;

    public AdminController(UserManager<AppUser> users)
    {
        _users = users;
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var allUsers = _users.Users.ToList();

        return this.Inertia("Admin/Users", new
        {
            users = allUsers.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.CreatedAt,
            })
        });
    }
}