using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Backend;

public static class MvcGenerator
{
    // ── Program.cs ────────────────────────────────────────────────────────────

    public static string ProgramCs(ProjectOptions opts)
    {
        var dbUsing = opts.Database switch
        {
            Database.Sqlite     => "Microsoft.EntityFrameworkCore",
            Database.PostgreSQL => "Microsoft.EntityFrameworkCore",
            Database.SqlServer  => "Microsoft.EntityFrameworkCore",
            _ => "Microsoft.EntityFrameworkCore"
        };

        var dbConfig = opts.Database switch
        {
            Database.Sqlite     => $"""opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db")""",
            Database.PostgreSQL => $"""opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"))""",
            Database.SqlServer  => $"""opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))""",
            _ => throw new ArgumentOutOfRangeException()
        };

        if (opts.IncludeAuth)
            return ProgramCsAuth(opts, dbConfig);

        return ProgramCsNoAuth(opts, dbConfig);
    }

    private static string ProgramCsAuth(ProjectOptions opts, string dbConfig) => $$"""
using InertiaSharp;
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Data;
using {{opts.Namespace}}.Models;
using {{opts.Namespace}}.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    {{dbConfig}});

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<AppUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 8;
        opt.Password.RequireNonAlphanumeric = false;
        opt.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath       = "/login";
    opt.LogoutPath      = "/logout";
    opt.AccessDeniedPath = "/403";
    opt.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Headers.ContainsKey("X-Inertia"))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    opt.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Headers.ContainsKey("X-Inertia"))
        {
            ctx.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// ── Authorization ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(Policies.CanEditContent, p => p.RequireRole(Roles.Admin, Roles.Editor));
    opt.AddPolicy(Policies.CanManageUsers, p => p.RequireRole(Roles.Admin));
    opt.AddPolicy(Policies.CanViewReports, p => p.RequireRole(Roles.Admin, Roles.Editor, Roles.Viewer));
});

// ── InertiaSharp ──────────────────────────────────────────────────────────────
var viteManifestPath = Path.Combine(builder.Environment.WebRootPath, ".vite", "manifest.json");
var assetVersion = File.Exists(viteManifestPath)
    ? Convert.ToHexString(System.Security.Cryptography.MD5.HashData(
          System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(viteManifestPath))))
    : builder.Environment.IsDevelopment() ? "dev" : "1";

builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";
    opt.Version  = assetVersion;
});

// ── MVC + Antiforgery ─────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(opt => opt.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

await SeedAsync(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Share auth props on every request
app.Use(async (ctx, next) =>
{
    var inertia = ctx.RequestServices.GetRequiredService<InertiaService>();
    var userMgr = ctx.RequestServices.GetRequiredService<UserManager<AppUser>>();
    var appUser = await userMgr.GetUserAsync(ctx.User);

    if (appUser is not null)
    {
        var roles = await userMgr.GetRolesAsync(appUser);
        inertia.Share("auth", new
        {
            user = new
            {
                appUser.Id,
                appUser.FullName,
                appUser.Email,
                appUser.AvatarPathFile,
                appUser.HasAvatar,
            },
            permissions = new
            {
                canEditContent = roles.Contains(Roles.Admin) || roles.Contains(Roles.Editor),
                canManageUsers = roles.Contains(Roles.Admin),
                canViewReports = true,
            },
        });
    }

    await next(ctx);
});

app.UseInertia();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseViteDevelopmentServer();
app.Run();

static async Task SeedAsync(WebApplication app)
{
    using var scope  = app.Services.CreateScope();
    var db           = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager  = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    await db.Database.MigrateAsync();

    foreach (var role in new[] { Roles.Admin, Roles.Editor, Roles.Viewer })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    const string adminEmail = "admin@demo.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new AppUser
        {
            UserName  = adminEmail,
            Email     = adminEmail,
            FirstName = "Admin",
            LastName  = "User",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, "Password123!");
        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
""";

    private static string ProgramCsNoAuth(ProjectOptions opts, string dbConfig) => $$"""
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    {{dbConfig}});

// ── InertiaSharp ──────────────────────────────────────────────────────────────
var viteManifestPath = Path.Combine(builder.Environment.WebRootPath, ".vite", "manifest.json");
var assetVersion = File.Exists(viteManifestPath)
    ? Convert.ToHexString(System.Security.Cryptography.MD5.HashData(
          System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(viteManifestPath))))
    : builder.Environment.IsDevelopment() ? "dev" : "1";

builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";
    opt.Version  = assetVersion;
});

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(opt => opt.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!db.Items.Any())
    {
        db.Items.AddRange(
            new() { Name = "First Item",  Description = "Hello from InertiaSharp!" },
            new() { Name = "Second Item", Description = "Edit me in HomeController." },
            new() { Name = "Third Item",  Description = "Add more rows as needed." }
        );
        await db.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseInertia();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseViteDevelopmentServer();
app.Run();
""";

    // ── Controllers ───────────────────────────────────────────────────────────

    public static string HomeController(ProjectOptions opts)
    {
        if (opts.IncludeAuth)
        {
            return $$"""
using InertiaSharp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace {{opts.Namespace}}.Controllers;

[Route("")]
public class HomeController : Controller
{
    [HttpGet("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect("/dashboard");

        return Redirect("/login");
    }

    [HttpGet("403")]
    [AllowAnonymous]
    public IActionResult Forbidden() => this.Inertia("Errors/Forbidden");

    [HttpGet("error")]
    [AllowAnonymous]
    public IActionResult Error() => this.Inertia("Errors/ServerError");
}
""";
        }

        return $$"""
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace {{opts.Namespace}}.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var items = await _db.Items
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Name, x.Description, x.CreatedAt })
            .ToListAsync();

        return this.Inertia("Home", new { items });
    }
}
""";
    }

    public static string AuthController(ProjectOptions opts) => $$"""
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Contracts;
using {{opts.Namespace}}.Models;
using {{opts.Namespace}}.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace {{opts.Namespace}}.Controllers;

[Route("")]
public class AuthController : Controller
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser>  _users;

    public AuthController(SignInManager<AppUser> signIn, UserManager<AppUser> users)
    {
        _signIn = signIn;
        _users  = users;
    }

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
    public async Task<IActionResult> LoginPost([FromBody] LoginRequest req)
    {
        var result = await _signIn.PasswordSignInAsync(req.Email, req.Password, req.Remember, lockoutOnFailure: true);

        if (result.Succeeded)
            return Redirect("/dashboard");

        var errors = new Dictionary<string, string>
        {
            ["email"] = result.IsLockedOut ? "Account locked. Try again later." : "Invalid credentials."
        };

        return this.Inertia("Auth/Login", new { errors });
    }

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
    public async Task<IActionResult> RegisterPost([FromBody] RegisterRequest req)
    {
        if (req.Password != req.PasswordConfirmation)
            return this.Inertia("Auth/Register", new
            {
                errors = new { passwordConfirmation = "Passwords do not match." }
            });

        var user = new AppUser
        {
            UserName  = req.Email,
            Email     = req.Email,
            FirstName = req.FirstName,
            LastName  = req.LastName,
        };

        var result = await _users.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
            return this.Inertia("Auth/Register", new { errors });
        }

        await _users.AddToRoleAsync(user, Roles.Viewer);
        await _signIn.SignInAsync(user, isPersistent: false);
        return Redirect("/dashboard");
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return Redirect("/login");
    }
}
""";

    public static string DashboardController(ProjectOptions opts) => $$"""
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Models;
using {{opts.Namespace}}.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace {{opts.Namespace}}.Controllers;

[Authorize]
[Route("")]
public class DashboardController : Controller
{
    private readonly UserManager<AppUser> _users;

    public DashboardController(UserManager<AppUser> users) => _users = users;

    [HttpGet("dashboard")]
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
            stats = new { totalUsers = 42, activeToday = 7, pendingTasks = 3 },
            permissions = new
            {
                canEditContent = roles.Contains(Roles.Admin) || roles.Contains(Roles.Editor),
                canManageUsers = roles.Contains(Roles.Admin),
                canViewReports = true,
            },
        });
    }
}
""";

    public static string ProfileController(ProjectOptions opts) => $$"""
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Contracts;
using {{opts.Namespace}}.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace {{opts.Namespace}}.Controllers;

[Authorize]
[Route("profile")]
public class ProfileController : Controller
{
    private readonly UserManager<AppUser> _users;

    public ProfileController(UserManager<AppUser> users) => _users = users;

    [HttpGet("")]
    public async Task<IActionResult> Edit()
    {
        var user  = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();
        var flash = TempData["_flash_success"] as string;

        return this.Inertia("Profile/Edit", new
        {
            user = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.Bio,
                user.PhoneNumber,
                user.AvatarPathFile,
                user.HasAvatar,
            },
            flash,
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest req)
    {
        var user = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();

        if (req.Email != user.Email)
        {
            var existing = await _users.FindByEmailAsync(req.Email);
            if (existing is not null && existing.Id != user.Id)
                return this.Inertia("Profile/Edit", new
                {
                    user = req,
                    errors = new { email = "Email is already taken." }
                });
        }

        user.FirstName   = req.FirstName;
        user.LastName    = req.LastName;
        user.Email       = req.Email;
        user.UserName    = req.Email;
        user.Bio         = req.Bio;
        user.PhoneNumber = req.PhoneNumber;

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
            return this.Inertia("Profile/Edit", new { user = req, errors });
        }

        TempData["_flash_success"] = "Profile updated successfully.";
        return Redirect("/profile");
    }

    [HttpPost("password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Password([FromBody] ChangePasswordRequest req)
    {
        var user   = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();

        if (req.NewPassword != req.NewPasswordConfirmation)
            return this.Inertia("Profile/Edit", new
            {
                user,
                errors = new { newPasswordConfirmation = "Passwords do not match." }
            });

        var result = await _users.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
            return this.Inertia("Profile/Edit", new { user, errors });
        }

        TempData["_flash_success"] = "Password changed successfully.";
        return Redirect("/profile");
    }

    [HttpPost("avatar")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Avatar([FromForm] IFormFile? avatar)
    {
        if (avatar is null || avatar.Length == 0)
            return this.Inertia("Profile/Edit", new
            {
                errors = new { avatar = "No file was uploaded." }
            });

        var user    = await _users.GetUserAsync(User) ?? throw new InvalidOperationException();
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);

        var fileName = $"{user.Id}{Path.GetExtension(avatar.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await avatar.CopyToAsync(stream);

        user.AvatarPathFile = $"uploads/{fileName}";
        await _users.UpdateAsync(user);

        TempData["_flash_success"] = "Avatar updated successfully.";
        return Redirect("/profile");
    }
}
""";

    public static string AdminController(ProjectOptions opts) => $$"""
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Models;
using {{opts.Namespace}}.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace {{opts.Namespace}}.Controllers;

[Authorize(Policy = Policies.CanManageUsers)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _users;

    public AdminController(UserManager<AppUser> users) => _users = users;

    [HttpGet("users")]
    public IActionResult Users()
    {
        var users = _users.Users.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.CreatedAt,
        }).ToList();

        return this.Inertia("Admin/Users", new { users });
    }
}
""";
}
