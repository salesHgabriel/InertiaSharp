using InertiaSharp;
using InertiaSharp.Extensions;
using InertiaSharp.Sample.Data;
using InertiaSharp.Sample.Models;
using InertiaSharp.Sample.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor();

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
                  ?? "Data Source=app.db"));

// ── Identity (cookie-based auth) ────────────────────────────────────────────
builder.Services
    .AddIdentity<AppUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 8;
        opt.Password.RequireNonAlphanumeric = false;
        opt.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie paths so Inertia redirects work correctly
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath      = "/login";
    opt.LogoutPath     = "/logout";
    opt.AccessDeniedPath = "/403";
    // Return 401/403 JSON for Inertia XHR, redirect for full-page
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

// ── Authorization Policies ──────────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(Policies.CanEditContent,  p => p.RequireRole(Roles.Admin, Roles.Editor));
    opt.AddPolicy(Policies.CanManageUsers,  p => p.RequireRole(Roles.Admin));
    opt.AddPolicy(Policies.CanViewReports,  p => p.RequireRole(Roles.Admin, Roles.Editor, Roles.Viewer));
});

// ── InertiaSharp ────────────────────────────────────────────────────────────
// Compute asset version from the frontend manifest (set after build)
var viteManifestPath = Path.Combine(builder.Environment.WebRootPath, ".vite", "manifest.json");
var assetVersion = File.Exists(viteManifestPath)
    ? Convert.ToHexString(System.Security.Cryptography.MD5.HashData(
          System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(viteManifestPath))))
    : builder.Environment.IsDevelopment() ? "dev" : "1";

builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";       // → Views/Shared/App.cshtml
    opt.Version  = assetVersion;
});

// ── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Antiforgery ─────────────────────────────────────────────────────────────
builder.Services.AddAntiforgery(opt => opt.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

// ── Seed database ────────────────────────────────────────────────────────────
await SeedAsync(app);

// ── Middleware pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();



// Inertia middleware must come after UseRouting, before UseAuthentication
app.UseInertia();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Use(async (ctx, next) =>
{
    // Share auth data on every request in this group (like middleware)
    var inertia   = ctx.Request.HttpContext.RequestServices.GetRequiredService<InertiaService>();
    var userMgr   = ctx.Request.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
    var principal = ctx.Request.HttpContext.User;
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

    await next(ctx);
});

app.UseViteDevelopmentServer();
app.Run();

// ── Seeder ───────────────────────────────────────────────────────────────────
static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    await db.Database.MigrateAsync();

    // Seed roles
    foreach (var role in new[] { Roles.Admin, Roles.Editor, Roles.Viewer })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed demo admin
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
