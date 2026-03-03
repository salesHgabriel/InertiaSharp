using System.Security.Claims;
using InertiaSharp.Extensions;
using InertiaSharp.Sample.Models;
using InertiaSharp.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InertiaSharp.Sample.Controllers;

[Authorize]
[Route("profile")]
public class ProfileController : Controller
{
    private readonly UserManager<AppUser> _users;


    public ProfileController(UserManager<AppUser> users) => _users = users;

    // ── View profile ─────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Edit()
    {
        var user = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();


        var flash = HttpContext.GetFlashMessage("_flash_success");

        if (flash is not null)
            HttpContext.RemoveFlashMessage("_flash_success");

        return this.Inertia("Profile/Edit", new
        {
            user = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.Bio,
                user.PhoneNumber,
            },
            flash
        });
    }

    // ── Update profile info ──────────────────────────────────────────────────

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        [FromBody] UpdateProfileRequest request)
    {
        var user = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();
        var errors = new Dictionary<string, string>();

        // Email uniqueness check
        if (request.Email != user.Email)
        {
            var existing = await _users.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != user.Id)
                errors["email"] = "Email is already taken.";
        }

        if (errors.Any())
        {
            return this.Inertia("Profile/Edit", new
            {
                user = new { request.FirstName, request.LastName, request.Email, request.Bio, request.PhoneNumber },
                errors
            });
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.Bio = request.Bio;
        user.PhoneNumber = request.PhoneNumber;

        var result = await _users.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                errors[e.Code] = e.Description;

            return this.Inertia("Profile/Edit", new
            {
                user = new { request.FirstName, request.LastName, request.Email, request.Bio, request.PhoneNumber },
                errors
            });
        }


        HttpContext.AddFlashMessage("_flash_success", "Profile updated successfully.");
        return Redirect("/profile");
    }

    // ── Change password ──────────────────────────────────────────────────────

    [HttpPost("password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (request.NewPassword != request.NewPasswordConfirmation)
        {
            errors["newPasswordConfirmation"] = "Passwords do not match.";
            return this.Inertia("Profile/Edit", new { errors });
        }

        var user = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();
        var result = await _users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                errors[e.Code] = e.Description;

            return this.Inertia("Profile/Edit", new { errors });
        }


        HttpContext.AddFlashMessage("_flash_success", "Password changed successfully.");
        return Redirect("/profile");
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UpdateAvatar(
        [FromForm] IFormFile? avatar)
    {
        if (avatar is null || avatar.Length == 0)
            return this.Inertia("Profile/Edit", new
            {
                errors = new { avatar = "Nenhum arquivo enviado." }
            });

        var user = await _users.GetUserAsync(HttpContext.User) ?? throw new InvalidOperationException();

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);

        var fileName = $"{user!.Id}{Path.GetExtension(avatar.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await avatar.CopyToAsync(stream);

        user.AvatarPathFile = $"uploads/{fileName}";

        await _users.UpdateAsync(user);

        HttpContext.AddFlashMessage("_flash_success", "Password changed successfully.");

        return Redirect("/profile");
    }
}