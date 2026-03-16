using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Backend;

/// <summary>
/// Generates files shared by both MVC and Minimal API backends:
/// AppUser, AppDbContext, SeedData / SampleItem, App.cshtml, appsettings.
/// </summary>
public static class SharedBackendGenerator
{
    // ── appsettings.json ─────────────────────────────────────────────────────

    public static string AppSettings(ProjectOptions opts) => $$"""
{
  "ConnectionStrings": {
    "Default": "{{opts.DbConnectionString}}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
""";

    public static string AppSettingsDev(ProjectOptions opts) => $$"""
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
""";

    // ── Views/Shared/App.cshtml ───────────────────────────────────────────────

    public static string AppCshtml(ProjectOptions opts)
    {
        var vitePort = opts.VitePort;
        var entryExt = opts.Frontend switch
        {
            Frontend.Vue    => "ts",
            Frontend.React  => "tsx",
            Frontend.Svelte => "ts",
            _ => "ts"
        };

        return $$"""
@using InertiaSharp.TagHelpers
@using Microsoft.AspNetCore.Antiforgery
@using Microsoft.AspNetCore.Hosting
@inject IAntiforgery        AntiforgeryService
@inject IWebHostEnvironment HostEnv
@addTagHelper *, InertiaSharp
<!DOCTYPE html>
<html lang="en" class="h-full">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>{{opts.ProjectName}}</title>
    <meta name="csrf-token" content="@AntiforgeryService.GetAndStoreTokens(Context).RequestToken" />

    @if (HostEnv.IsDevelopment())
    {
        <script type="module" src="http://localhost:{{vitePort}}/@@vite/client"></script>
        <script type="module" src="http://localhost:{{vitePort}}/src/app.{{entryExt}}"></script>
    }
    else
    {
        <link rel="stylesheet" href="~/dist/app.css" asp-append-version="true" />
        <script type="module" src="~/dist/app.js" asp-append-version="true"></script>
    }
</head>
<body class="h-full bg-background font-sans antialiased">
    <inertia />
</body>
</html>
""";
    }

    // ── Models/AppUser.cs (auth only) ─────────────────────────────────────────

    public static string AppUserModel(ProjectOptions opts) => $$"""
using Microsoft.AspNetCore.Identity;

namespace {{opts.Namespace}}.Models;

public class AppUser : IdentityUser
{
    public string  FirstName     { get; set; } = string.Empty;
    public string  LastName      { get; set; } = string.Empty;
    public string? Bio           { get; set; }
    public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
    public string? AvatarPathFile { get; set; }

    public string FullName  => $"{FirstName} {LastName}".Trim();
    public bool   HasAvatar => AvatarPathFile is not null;
}
""";

    // ── Models/SampleItem.cs (no-auth only) ───────────────────────────────────

    public static string SampleItemModel(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Models;

public class SampleItem
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
}
""";

    // ── Data/AppDbContext.cs ──────────────────────────────────────────────────

    public static string AppDbContext(ProjectOptions opts)
    {
        if (opts.IncludeAuth)
        {
            return $$"""
using {{opts.Namespace}}.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace {{opts.Namespace}}.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().ToTable("Users");
    }
}
""";
        }

        return $$"""
using {{opts.Namespace}}.Models;
using Microsoft.EntityFrameworkCore;

namespace {{opts.Namespace}}.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SampleItem> Items => Set<SampleItem>();
}
""";
    }

    // ── Permissions/Roles.cs & Policies.cs (auth only) ───────────────────────

    public static string RolesClass(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Permissions;

public static class Roles
{
    public const string Admin  = "Admin";
    public const string Editor = "Editor";
    public const string Viewer = "Viewer";
}
""";

    public static string PoliciesClass(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Permissions;

public static class Policies
{
    public const string CanEditContent = "CanEditContent";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanViewReports = "CanViewReports";
}
""";

    // ── Contracts (shared request models) ────────────────────────────────────

    public static string LoginRequest(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Contracts;

public record LoginRequest(string Email, string Password, bool Remember = false);
""";

    public static string RegisterRequest(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Contracts;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PasswordConfirmation);
""";

    public static string UpdateProfileRequest(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Contracts;

public record UpdateProfileRequest(
    string  FirstName,
    string  LastName,
    string  Email,
    string? Bio,
    string? PhoneNumber);
""";

    public static string ChangePasswordRequest(ProjectOptions opts) => $$"""
namespace {{opts.Namespace}}.Contracts;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string NewPasswordConfirmation);
""";

    // ── Properties/launchSettings.json ───────────────────────────────────────

    public static string LaunchSettings() => """
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Development": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "Docker": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}",
      "publishAllPorts": true
    }
  }
}
""";

    // ── run-dev.sh ────────────────────────────────────────────────────────────

    public static string RunDevScript(ProjectOptions opts) => $$"""
#!/usr/bin/env bash
# Run .NET backend + Vite frontend concurrently in development
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CLIENT_APP="$SCRIPT_DIR/ClientApp"

echo "Installing npm dependencies..."
cd "$CLIENT_APP" && npm install && cd "$SCRIPT_DIR"

echo "Restoring .NET packages..."
dotnet restore

echo ""
echo "Starting development servers..."
echo "  .NET  → https://localhost:5001"
echo "  Vite  → http://localhost:{{opts.VitePort}}"
echo ""

trap 'kill 0' EXIT

dotnet watch run --no-hot-reload &
(cd "$CLIENT_APP" && npm run dev) &

wait
""";
}
