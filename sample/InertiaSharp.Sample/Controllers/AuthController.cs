using InertiaSharp.Extensions;
using InertiaSharp.Sample.Models;
using InertiaSharp.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InertiaSharp.Sample.Controllers;


[Route("")]
public class AuthController : Controller
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser>  _users;

    public record LoginRequest(string Email, string Password, bool Remember = false);
    
    public AuthController(SignInManager<AppUser> signIn, UserManager<AppUser> users)
    {
        _signIn = signIn;
        _users  = users;
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (_signIn.IsSignedIn(User))
            return Redirect("/dashboard");

        return this.Inertia("Auth/Login");
    }

    
    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginPost(
        [FromBody] LoginRequest request)
    {
        var result = await _signIn.PasswordSignInAsync(request.Email, request.Password, request.Remember, lockoutOnFailure: true);

        if (result.Succeeded)
            return Redirect("/dashboard");

        var errors = new Dictionary<string, string>();

        if (result.IsLockedOut)
            errors["email"] = "Account locked. Try again later.";
        else
            errors["email"] = "Invalid credentials.";

        return this.Inertia("Auth/Login", new { errors });
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (_signIn.IsSignedIn(User))
            return Redirect("/dashboard");

        return this.Inertia("Auth/Register");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPost(
        [FromServices] IHttpContextAccessor context,
        [FromBody] RegisterRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (request.Password != request.PasswordConfirmation)
            errors["passwordConfirmation"] = "Passwords do not match.";

        if (errors.Any())
            return this.Inertia("Auth/Register", new { errors });

        var user = new AppUser
        {
            UserName  = request.Email,
            Email     = request.Email,
            FirstName = request.FirstName,
            LastName  = request.LastName,
        };

        var result = await _users.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                errors[error.Code] = error.Description;

            return this.Inertia("Auth/Register", new { errors });
        }

        await _signIn.SignInAsync(user, isPersistent: false);
        return Redirect("/dashboard");
    }

    // ── Logout ───────────────────────────────────────────────────────────────

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return Redirect("/login");
    }
}
