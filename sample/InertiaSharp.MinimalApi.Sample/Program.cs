
// ┌─────────────────────────────────────────────────────────────────────────┐
// │  InertiaSharp — Minimal API Sample                                       │
// │                                                                          │
// │  Demonstrates every supported pattern:                                   │
// │   • Results.Extensions.Inertia(component, props)                        │
// │   • app.MapInertia(route, component)   — static pages                   │
// │   • app.MapInertiaGroup(prefix)        — grouped + authorized routes     │
// │   • Inline handlers with async DI injection                              │
// │   • ASP.NET Core Identity + cookie auth                                  │
// │   • Role-based permissions as props                                      │
// └─────────────────────────────────────────────────────────────────────────┘


using InertiaSharp.Extensions;
using InertiaSharp.MinimalApi.Sample.Data;
using InertiaSharp.MinimalApi.Sample.Models;
using InertiaSharp.MinimalApi.Sample.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InertiaSharp;
using InertiaSharp.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=minimal.db"));

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

    // Return HTTP status codes (not redirects) on Inertia XHR requests
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

// ── Authorization policies ────────────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(Policies.CanEditContent,  p => p.RequireRole(Roles.Admin, Roles.Editor));
    opt.AddPolicy(Policies.CanManageUsers,  p => p.RequireRole(Roles.Admin));
    opt.AddPolicy(Policies.CanViewReports,  p => p.RequireRole(Roles.Admin, Roles.Editor, Roles.Viewer));
});

// ── Inertia ───────────────────────────────────────────────────────────────────
var viteManifest = Path.Combine(builder.Environment.WebRootPath, ".vite", "manifest.json");
builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";
    opt.Version  = File.Exists(viteManifest)
        ? Convert.ToHexString(
            System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(viteManifest))))
        : "dev";
});

// ── MVC Razor views (required to render the shell App.cshtml) ─────────────────
// Note: We still need AddControllersWithViews (or AddRazorPages) for the
// Razor view engine, even in a Minimal API project.
// The views are ONLY used for the shell — no MVC controllers are defined here.
builder.Services.AddControllersWithViews();

// ── Antiforgery ───────────────────────────────────────────────────────────────
builder.Services.AddAntiforgery(opt => { opt.HeaderName = "X-CSRF-TOKEN"; opt.SuppressXFrameOptionsHeader = false; });

var app = builder.Build();

// ── Seed ──────────────────────────────────────────────────────────────────────
await SeedAsync(app);

// ── Pipeline ──────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseInertia();          // ← InertiaSharp middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();


// ─────────────────────────────────────────────────────────────────────────────
// ROUTES — Three patterns shown side-by-side
// ─────────────────────────────────────────────────────────────────────────────

// ── Pattern 1: Static page (MapInertia shorthand) ─────────────────────────────
// No handler needed — the component gets no dynamic props.
// Perfect for marketing pages, error pages, about screens, etc.
// app.MapInertia("/", "Home")
//    .WithInertiaDisplayName("Home");

app.MapInertia("/about", "Marketing/About")
   .WithInertiaDisplayName("Marketing/About");

app.MapInertia("/403", "Errors/Forbidden").AllowAnonymous();
app.MapInertia("/error", "Errors/ServerError").AllowAnonymous();

// ── Pattern 2: Results.Extensions.Inertia() inside a handler ─────────────────
// Full access to DI, async, request data. The idiomatic Minimal API way.

app.MapGet("/", (ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true
            ? Results.Redirect("/dashboard")
            : Results.Extensions.Inertia("Auth/Login"))
    .AllowAnonymous()
    .WithInertiaDisplayName("Auth/Login");

app.MapGet("/login", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Redirect("/dashboard")
        : Results.Extensions.Inertia("Auth/Login"))
   .AllowAnonymous()
   .WithInertiaDisplayName("Auth/Login");

app.MapPost("/login", async (
    HttpContext ctx,
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
})
.AllowAnonymous();

app.MapGet("/register", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Redirect("/dashboard")
        : Results.Extensions.Inertia("Auth/Register"))
   .AllowAnonymous()
   .WithInertiaDisplayName("Auth/Register");

app.MapPost("/register", async (
    HttpContext ctx,
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

    var resultUser  = await users.CreateAsync(user, req.Password);
    
    var resultRole = await users.AddToRoleAsync(user, Roles.Viewer);
    
    if (!resultUser.Succeeded)
    {
        var errors = resultUser.Errors.ToDictionary(e => e.Code, e => e.Description);
        return Results.Extensions.Inertia("Auth/Register", new { errors });
    }
    
    if (!resultRole.Succeeded)
    {
        var errors = resultRole.Errors.ToDictionary(e => e.Code, e => e.Description);
        return Results.Extensions.Inertia("Auth/Register", new { errors });
    }

    await signIn.SignInAsync(user, isPersistent: false);
    return Results.Redirect("/dashboard");
})
.AllowAnonymous();


app.MapPost("/logout", async (
    SignInManager<AppUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.Redirect("/login");
})
.RequireAuthorization();

// ── Pattern 3: MapInertiaGroup — grouped, authorized endpoints ────────────────
// All routes in this group require authentication automatically.
// The InertiaService is injected per-request to share auth props.

var authenticated = app
    .MapInertiaGroup("/")
    .RequireAuthorization()
    .AddEndpointFilter(async (ctx, next) =>
    {
        // Share auth data on every request in this group (like middleware)
        var inertia   = ctx.HttpContext.RequestServices.GetRequiredService<InertiaService>();
        var userMgr   = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
        var principal = ctx.HttpContext.User;
        var appUser   = await userMgr.GetUserAsync(principal);

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

authenticated.MapGet("dashboard", async (
    UserManager<AppUser> users,
    ClaimsPrincipal principal) =>
{
    var user  = await users.GetUserAsync(principal) ?? throw new InvalidOperationException();
    var roles = await users.GetRolesAsync(user);

    return Results.Extensions.Inertia("Dashboard", new
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
            canViewReports = true, // all authenticated users
        },
    });
})
.WithInertiaDisplayName("Dashboard");


authenticated.MapGet("profile", async (
    UserManager<AppUser> users,
    ClaimsPrincipal principal,
    HttpContext ctx) =>
{
    var user = await users.GetUserAsync(principal) ?? throw new InvalidOperationException();

    var flash = ctx.GetFlashMessage("_flash_success");
 
    if (flash is not null)
        ctx.RemoveFlashMessage("_flash_success");

    return Results.Extensions.Inertia("Profile/Edit", new
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
})
.WithInertiaDisplayName("Profile/Edit");

authenticated.MapPost("profile", async (
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
        {
            return Results.Extensions.Inertia("Profile/Edit", new
            {
                user = req,
                errors = new { email = "Email is already taken." }
            });
        }
    }

    user.FirstName   = req.FirstName;
    user.LastName    = req.LastName;
    user.Email       = req.Email;
    user.UserName    = req.Email;
    user.Bio         = req.Bio;
    user.PhoneNumber = req.PhoneNumber;
    

    var result = await users.UpdateAsync(user);
    if (!result.Succeeded)
    {
        var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
        return Results.Extensions.Inertia("Profile/Edit", new { user = req, errors });
    }

    ctx.AddFlashMessage("_flash_success", "Profile updated successfully.");
    return Results.Redirect("/profile");
});

authenticated.MapPost("profile/password", async (
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
                errors = new { avatar = "Nenhum arquivo enviado." }
            });

        var user = await users.GetUserAsync(principal)!;

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);

        var fileName = $"{user!.Id}{Path.GetExtension((string?)avatar.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        await using var stream = File.Create(filePath);
        await avatar.CopyToAsync(stream);
        
        user.AvatarPathFile =  $"uploads/{fileName}";
        
       await users.UpdateAsync(user);
       
       ctx.AddFlashMessage("_flash_success", "Added avatar profile successfully.");

        return Results.Redirect("/profile");
    })
    .RequireAuthorization()
    .DisableAntiforgery();


authenticated
    .MapGet("admin/users",
        async (UserManager<AppUser> users) =>
        {
            var allUsers = users.Users.ToList();
            return Results.Extensions.Inertia("Admin/Users", new { users = Enumerable.Select(allUsers, u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.CreatedAt,
            })});
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


