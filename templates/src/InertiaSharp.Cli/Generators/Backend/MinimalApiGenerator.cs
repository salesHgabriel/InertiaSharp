using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Backend;

public static class MinimalApiGenerator
{
    public static string ProgramCs(ProjectOptions opts)
    {
        var dbConfig = opts.Database switch
        {
            Database.Sqlite     => $"""opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db")""",
            Database.PostgreSQL => $"""opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"))""",
            Database.SqlServer  => $"""opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))""",
            _ => throw new ArgumentOutOfRangeException()
        };

        return opts.IncludeAuth
            ? ProgramCsAuth(opts, dbConfig)
            : ProgramCsNoAuth(opts, dbConfig);
    }

    private static string ProgramCsAuth(ProjectOptions opts, string dbConfig) => $$"""
using InertiaSharp;
using InertiaSharp.Extensions;
using {{opts.Namespace}}.Contracts;
using {{opts.Namespace}}.Data;
using {{opts.Namespace}}.Models;
using {{opts.Namespace}}.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

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
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    opt.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Headers.ContainsKey("X-Inertia"))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
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
var viteManifest = Path.Combine(builder.Environment.WebRootPath, ".vite", "manifest.json");
builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";
    opt.Version  = File.Exists(viteManifest)
        ? Convert.ToHexString(System.Security.Cryptography.MD5.HashData(
              System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(viteManifest))))
        : "dev";
});

// Razor view engine (required to render App.cshtml shell even in Minimal API)
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(opt => { opt.HeaderName = "X-CSRF-TOKEN"; });

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
app.UseInertia();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ── Public routes ─────────────────────────────────────────────────────────────
app.MapInertia("/403",   "Errors/Forbidden").AllowAnonymous();
app.MapInertia("/error", "Errors/ServerError").AllowAnonymous();

app.MapGet("/", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Redirect("/dashboard")
        : Results.Extensions.Inertia("Auth/Login"))
   .AllowAnonymous();

app.MapGet("/login", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Redirect("/dashboard")
        : Results.Extensions.Inertia("Auth/Login"))
   .AllowAnonymous();

app.MapPost("/login", async (
    SignInManager<AppUser> signIn,
    [Microsoft.AspNetCore.Mvc.FromBody] LoginRequest req) =>
{
    var result = await signIn.PasswordSignInAsync(req.Email, req.Password, req.Remember, lockoutOnFailure: true);

    if (result.Succeeded)
        return Results.Redirect("/dashboard");

    var errors = new Dictionary<string, string>
    {
        ["email"] = result.IsLockedOut ? "Account locked. Try again later." : "Invalid credentials."
    };

    return Results.Extensions.Inertia("Auth/Login", new { errors });
}).AllowAnonymous();

app.MapGet("/register", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Redirect("/dashboard")
        : Results.Extensions.Inertia("Auth/Register"))
   .AllowAnonymous();

app.MapPost("/register", async (
    UserManager<AppUser> users,
    SignInManager<AppUser> signIn,
    [Microsoft.AspNetCore.Mvc.FromBody] RegisterRequest req) =>
{
    if (req.Password != req.PasswordConfirmation)
        return Results.Extensions.Inertia("Auth/Register", new
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

    var result = await users.CreateAsync(user, req.Password);
    if (!result.Succeeded)
    {
        var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
        return Results.Extensions.Inertia("Auth/Register", new { errors });
    }

    await users.AddToRoleAsync(user, Roles.Viewer);
    await signIn.SignInAsync(user, isPersistent: false);
    return Results.Redirect("/dashboard");
}).AllowAnonymous();

app.MapPost("/logout", async (SignInManager<AppUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.Redirect("/login");
}).RequireAuthorization();

// ── Authenticated group ───────────────────────────────────────────────────────
var auth = app.MapInertiaGroup("/")
    .RequireAuthorization()
    .AddEndpointFilter(async (ctx, next) =>
    {
        var inertia = ctx.HttpContext.RequestServices.GetRequiredService<InertiaService>();
        var userMgr = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
        var appUser = await userMgr.GetUserAsync(ctx.HttpContext.User);

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

        return await next(ctx);
    });

auth.MapGet("dashboard", async (UserManager<AppUser> users, ClaimsPrincipal principal) =>
{
    var user  = await users.GetUserAsync(principal) ?? throw new InvalidOperationException();
    var roles = await users.GetRolesAsync(user);

    return Results.Extensions.Inertia("Dashboard", new
    {
        user = new { user.Id, user.FullName, user.Email, user.CreatedAt, roles },
        stats = new { totalUsers = 42, activeToday = 7, pendingTasks = 3 },
        permissions = new
        {
            canEditContent = roles.Contains(Roles.Admin) || roles.Contains(Roles.Editor),
            canManageUsers = roles.Contains(Roles.Admin),
            canViewReports = true,
        },
    });
}).WithInertiaDisplayName("Dashboard");

auth.MapGet("profile", async (UserManager<AppUser> users, ClaimsPrincipal principal, HttpContext ctx) =>
{
    var user  = await users.GetUserAsync(principal) ?? throw new InvalidOperationException();
    var flash = ctx.GetFlashMessage("_flash_success");
    if (flash is not null) ctx.RemoveFlashMessage("_flash_success");

    return Results.Extensions.Inertia("Profile/Edit", new
    {
        user = new
        {
            user.FirstName, user.LastName, user.Email,
            user.Bio, user.PhoneNumber, user.AvatarPathFile, user.HasAvatar,
        },
        flash,
    });
}).WithInertiaDisplayName("Profile/Edit");

auth.MapPost("profile", async (
    HttpContext ctx,
    UserManager<AppUser> users,
    ClaimsPrincipal principal,
    [Microsoft.AspNetCore.Mvc.FromBody] UpdateProfileRequest req) =>
{
    var user = await users.GetUserAsync(principal) ?? throw new InvalidOperationException();

    if (req.Email != user.Email)
    {
        var existing = await users.FindByEmailAsync(req.Email);
        if (existing is not null && existing.Id != user.Id)
            return Results.Extensions.Inertia("Profile/Edit", new
            {
                user = req,
                errors = new { email = "Email is already taken." }
            });
    }

    user.FirstName = req.FirstName; user.LastName = req.LastName;
    user.Email = req.Email; user.UserName = req.Email;
    user.Bio = req.Bio; user.PhoneNumber = req.PhoneNumber;

    var result = await users.UpdateAsync(user);
    if (!result.Succeeded)
    {
        var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
        return Results.Extensions.Inertia("Profile/Edit", new { user = req, errors });
    }

    ctx.AddFlashMessage("_flash_success", "Profile updated successfully.");
    return Results.Redirect("/profile");
});

auth.MapPost("profile/password", async (
    HttpContext ctx,
    UserManager<AppUser> users,
    ClaimsPrincipal principal,
    [Microsoft.AspNetCore.Mvc.FromBody] ChangePasswordRequest req) =>
{
    if (req.NewPassword != req.NewPasswordConfirmation)
        return Results.Extensions.Inertia("Profile/Edit", new
        {
            errors = new { newPasswordConfirmation = "Passwords do not match." }
        });

    var user   = await users.GetUserAsync(principal) ?? throw new InvalidOperationException();
    var result = await users.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);

    if (!result.Succeeded)
    {
        var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
        return Results.Extensions.Inertia("Profile/Edit", new { errors });
    }

    ctx.AddFlashMessage("_flash_success", "Password changed successfully.");
    return Results.Redirect("/profile");
});

app.MapPost("/profile/avatar", async (
    HttpContext ctx,
    IFormFile? avatar,
    UserManager<AppUser> users,
    ClaimsPrincipal principal) =>
{
    if (avatar is null || avatar.Length == 0)
        return Results.Extensions.Inertia("Profile/Edit", new
        {
            errors = new { avatar = "No file was uploaded." }
        });

    var user    = await users.GetUserAsync(principal)!;
    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    Directory.CreateDirectory(uploads);

    var fileName = $"{user!.Id}{Path.GetExtension(avatar.FileName)}";
    var filePath = Path.Combine(uploads, fileName);

    await using var stream = File.Create(filePath);
    await avatar.CopyToAsync(stream);

    user.AvatarPathFile = $"uploads/{fileName}";
    await users.UpdateAsync(user);

    ctx.AddFlashMessage("_flash_success", "Avatar updated successfully.");
    return Results.Redirect("/profile");
}).RequireAuthorization().DisableAntiforgery();

auth.MapGet("admin/users", async (UserManager<AppUser> users) =>
{
    var all = users.Users.Select(u => new { u.Id, u.FullName, u.Email, u.CreatedAt }).ToList();
    return Results.Extensions.Inertia("Admin/Users", new { users = all });
})
.RequireAuthorization(Policies.CanManageUsers)
.WithInertiaDisplayName("Admin/Users");

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
            UserName = adminEmail, Email = adminEmail,
            FirstName = "Admin",  LastName = "User",
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
using {{opts.Namespace}}.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    {{dbConfig}});

// ── InertiaSharp ──────────────────────────────────────────────────────────────
var viteManifest = Path.Combine(builder.Environment.WebRootPath, ".vite", "manifest.json");
builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";
    opt.Version  = File.Exists(viteManifest)
        ? Convert.ToHexString(System.Security.Cryptography.MD5.HashData(
              System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(viteManifest))))
        : "dev";
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
            new SampleItem { Name = "First Item",  Description = "Hello from InertiaSharp!" },
            new SampleItem { Name = "Second Item", Description = "Edit me in Program.cs." },
            new SampleItem { Name = "Third Item",  Description = "Add more rows as needed." }
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
app.UseAntiforgery();

app.MapGet("/", async (AppDbContext db) =>
{
    var items = await db.Items
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new { x.Id, x.Name, x.Description, x.CreatedAt })
        .ToListAsync();

    return Results.Extensions.Inertia("Home", new { items });
});

app.UseViteDevelopmentServer();
app.Run();
""";
}
