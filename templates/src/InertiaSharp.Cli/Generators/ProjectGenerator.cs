using InertiaSharp.Cli.Generators.Backend;
using InertiaSharp.Cli.Generators.Views;
using InertiaSharp.Cli.Models;
using Spectre.Console;

namespace InertiaSharp.Cli.Generators;

public static class ProjectGenerator
{
    public static async Task GenerateAsync(ProjectOptions opts)
    {
        AnsiConsole.WriteLine();

        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var taskScaffold  = ctx.AddTask("[green]Scaffolding project[/]", maxValue: 100);
                var taskFrontend  = ctx.AddTask("[yellow]Generating frontend[/]", maxValue: 100);
                var taskNpm       = ctx.AddTask("[blue]Installing npm packages[/]", maxValue: 100);
                var taskDotnet    = ctx.AddTask("[violet]Restoring .NET packages[/]", maxValue: 100);

                // ── Backend + shared files ────────────────────────────────────
                ScaffoldProject(opts);
                taskScaffold.Value = 100;

                // ── Frontend files ────────────────────────────────────────────
                ScaffoldFrontend(opts);
                taskFrontend.Value = 100;

                // ── npm install ───────────────────────────────────────────────
                await RunProcessAsync("npm", "install", Path.Combine(opts.OutputDirectory, "ClientApp"));
                taskNpm.Value = 100;

                // ── dotnet restore ────────────────────────────────────────────
                await RunProcessAsync("dotnet", "restore", opts.OutputDirectory);
                taskDotnet.Value = 100;
            });

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[green]Project created successfully![/]").RuleStyle("green"));
        AnsiConsole.WriteLine();

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[grey]Project:[/]",   $"[white]{opts.ProjectName}[/]");
        grid.AddRow("[grey]Location:[/]",  $"[grey]{opts.OutputDirectory}[/]");
        grid.AddRow("[grey]API style:[/]", $"[white]{opts.ApiStyle}[/]");
        grid.AddRow("[grey]Database:[/]",  $"[white]{opts.Database}[/]");
        grid.AddRow("[grey]Frontend:[/]",  $"[white]{opts.Frontend}[/]");
        grid.AddRow("[grey]Auth:[/]",      opts.IncludeAuth ? "[green]Identity[/]" : "[grey]None[/]");
        AnsiConsole.Write(grid);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Next steps:[/]");
        AnsiConsole.MarkupLine($"  [grey]cd[/] [white]{opts.ProjectName}[/]");
        AnsiConsole.MarkupLine($"  [grey]bash[/] [white]dotnet build[/]");

        if (opts.IncludeAuth)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Then run migrations:[/]");
            AnsiConsole.MarkupLine("  [grey]dotnet ef migrations add InitialCreate[/]");
            AnsiConsole.MarkupLine("  [grey](auto-applied on first run)[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Demo credentials:[/] [white]admin@demo.com[/] / [white]Password123![/]");
        }
    }

    // ── Backend scaffolding ───────────────────────────────────────────────────

    private static void ScaffoldProject(ProjectOptions opts)
    {
        var root = opts.OutputDirectory;
        Directory.CreateDirectory(root);

        // Project file
        WriteFile(root, $"{opts.ProjectName}.csproj", CsprojGenerator.Generate(opts));

        // appsettings
        WriteFile(root, "appsettings.json",             SharedBackendGenerator.AppSettings(opts));
        WriteFile(root, "appsettings.Development.json", SharedBackendGenerator.AppSettingsDev(opts));

        // launchSettings
        WriteFile(root, "Properties/launchSettings.json", SharedBackendGenerator.LaunchSettings());

        // Shell Razor view
        WriteFile(root, "Views/Shared/App.cshtml", SharedBackendGenerator.AppCshtml(opts));

        // Data layer
        if (opts.IncludeAuth)
        {
            WriteFile(root, "Models/AppUser.cs",  SharedBackendGenerator.AppUserModel(opts));
            WriteFile(root, "Data/AppDbContext.cs", SharedBackendGenerator.AppDbContext(opts));
            WriteFile(root, "Permissions/Roles.cs",    SharedBackendGenerator.RolesClass(opts));
            WriteFile(root, "Permissions/Policies.cs", SharedBackendGenerator.PoliciesClass(opts));
            WriteFile(root, "Contracts/LoginRequest.cs",          SharedBackendGenerator.LoginRequest(opts));
            WriteFile(root, "Contracts/RegisterRequest.cs",       SharedBackendGenerator.RegisterRequest(opts));
            WriteFile(root, "Contracts/UpdateProfileRequest.cs",  SharedBackendGenerator.UpdateProfileRequest(opts));
            WriteFile(root, "Contracts/ChangePasswordRequest.cs", SharedBackendGenerator.ChangePasswordRequest(opts));
        }
        else
        {
            WriteFile(root, "Models/SampleItem.cs",  SharedBackendGenerator.SampleItemModel(opts));
            WriteFile(root, "Data/AppDbContext.cs",  SharedBackendGenerator.AppDbContext(opts));
        }

        // Endpoints / controllers
        if (opts.ApiStyle == ApiStyle.Mvc)
        {
            WriteFile(root, "Program.cs", MvcGenerator.ProgramCs(opts));
            WriteFile(root, "Controllers/HomeController.cs", MvcGenerator.HomeController(opts));

            if (opts.IncludeAuth)
            {
                WriteFile(root, "Controllers/AuthController.cs",      MvcGenerator.AuthController(opts));
                WriteFile(root, "Controllers/DashboardController.cs", MvcGenerator.DashboardController(opts));
                WriteFile(root, "Controllers/ProfileController.cs",   MvcGenerator.ProfileController(opts));
                WriteFile(root, "Controllers/AdminController.cs",     MvcGenerator.AdminController(opts));
            }
        }
        else
        {
            WriteFile(root, "Program.cs", MinimalApiGenerator.ProgramCs(opts));
        }

        // run-dev.sh
        var devScript = SharedBackendGenerator.RunDevScript(opts);
        WriteFile(root, "run-dev.sh", devScript);
        MakeExecutable(Path.Combine(root, "run-dev.sh"));

        // .gitignore
        WriteFile(root, ".gitignore", GitIgnore());

        // Placeholder folders
        Directory.CreateDirectory(Path.Combine(root, "wwwroot"));
        Directory.CreateDirectory(Path.Combine(root, "Migrations"));
    }

    // ── Frontend scaffolding ──────────────────────────────────────────────────

    private static void ScaffoldFrontend(ProjectOptions opts)
    {
        var files = opts.Frontend switch
        {
            Frontend.Vue    => VueGenerator.Generate(opts),
            Frontend.React  => ReactGenerator.Generate(opts),
            Frontend.Svelte => SvelteGenerator.Generate(opts),
            _               => throw new ArgumentOutOfRangeException()
        };

        foreach (var (relativePath, content) in files)
        {
            WriteFile(opts.OutputDirectory, relativePath, content);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void WriteFile(string root, string relativePath, string content)
    {
        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content.TrimStart('\n'));
    }

    private static void MakeExecutable(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                var info = new System.Diagnostics.ProcessStartInfo("chmod", $"+x \"{path}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                };
                System.Diagnostics.Process.Start(info)?.WaitForExit();
            }
            catch { /* chmod not available, skip */ }
        }
    }

    private static async Task RunProcessAsync(string command, string args, string workingDir)
    {
        try
        {
            var info = new System.Diagnostics.ProcessStartInfo(command, args)
            {
                WorkingDirectory       = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
            };

            using var process = System.Diagnostics.Process.Start(info);
            if (process is null) return;

            await process.WaitForExitAsync();
        }
        catch
        {
            // Non-fatal: user can run manually
        }
    }

    private static string GitIgnore() => """
# .NET
bin/
obj/
*.user
*.suo
.vs/
appsettings.*.local.json

# EF Migrations (keep the folder, ignore the db file)
*.db
*.db-shm
*.db-wal

# Node
node_modules/
ClientApp/dist/

# wwwroot (built assets)
wwwroot/dist/
wwwroot/uploads/

# Vite
ClientApp/.vite/

# OS
.DS_Store
Thumbs.db
""";
}
