using InertiaSharp.Cli.Generators;
using InertiaSharp.Cli.Wizard;
using Spectre.Console;

// Handle --help / --version early
if (args.Length > 0 && (args[0] is "--help" or "-h"))
{
    PrintHelp();
    return;
}

if (args.Length > 0 && (args[0] is "--version" or "-v"))
{
    var version = ProjectService.GetLatestVersionAsync(ProjectService.ProjectInertiaSharpCli).Result;

    AnsiConsole.MarkupLine($"[grey]InertiaSharp CLI[/] [white]{version}[/]");
    return;
}

// Optional positional argument: project name
string? nameArg = null;

if (args.Length > 0 && !args[0].StartsWith('-'))
    nameArg = args[0];

try
{
    var opts = ProjectWizard.Run(nameArg);
    await ProjectGenerator.GenerateAsync(opts);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    Environment.Exit(1);
}

static void PrintHelp()
{
    AnsiConsole.MarkupLine("""
[bold]InertiaSharp CLI[/] — scaffold ASP.NET Core + Inertia.js starter projects

[bold]Usage:[/]
  inertiasharp [[project-name]]
  inertiasharp --help
  inertiasharp --version

[bold]Description:[/]
  Interactive wizard that asks you:
    1. API style   : MVC Controllers or Minimal API
    2. Database    : SQLite, PostgreSQL, or SQL Server
    3. Frontend    : Vue 3 + Reka UI, React + shadcn/ui, or Svelte + shadcn-svelte
    4. Auth        : ASP.NET Core Identity (login, register, profile, permissions)
                     or a simple home page with a database query

[bold]Examples:[/]
  inertiasharp
  inertiasharp MyApp
""");
}
