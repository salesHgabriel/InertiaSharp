using InertiaSharp.Cli.Models;
using Spectre.Console;

namespace InertiaSharp.Cli.Wizard;

public static class ProjectWizard
{
    public static ProjectOptions Run(string? nameArg)
    {
        AnsiConsole.Write(
            new FigletText("InertiaSharp")
                .Centered()
                .Color(Color.Violet));

        AnsiConsole.MarkupLine("[grey]The Inertia.js adapter for ASP.NET Core[/]\n");

        // ── Project name ─────────────────────────────────────────────────────
        var projectName = nameArg ?? AnsiConsole.Ask<string>("[bold]Project name:[/]");

        while (string.IsNullOrWhiteSpace(projectName))
        {
            AnsiConsole.MarkupLine("[red]Project name cannot be empty.[/]");
            projectName = AnsiConsole.Ask<string>("[bold]Project name:[/]");
        }

        // ── API style ────────────────────────────────────────────────────────
        var apiStyle = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Which [green]API style[/] would you like to use?[/]")
                .AddChoices(
                    "MVC Controllers",
                    "Minimal API")
                .HighlightStyle(new Style(Color.Green)));

        // ── Database ─────────────────────────────────────────────────────────
        var database = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Which [blue]database[/] would you like to use?[/]")
                .AddChoices(
                    "SQLite  (file-based, great for development)",
                    "PostgreSQL",
                    "SQL Server")
                .HighlightStyle(new Style(Color.Blue)));

        // ── Frontend ─────────────────────────────────────────────────────────
        var frontend = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Which [yellow]frontend[/] framework would you like?[/]")
                .AddChoices(
                    "Vue 3   + Reka UI  (shadcn-vue)",
                    "React   + shadcn/ui",
                    "Svelte  + shadcn-svelte")
                .HighlightStyle(new Style(Color.Yellow)));

        // ── Auth ─────────────────────────────────────────────────────────────
        var authChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Include [red]authentication[/]? (ASP.NET Core Identity)[/]")
                .AddChoices(
                    "Yes — login, register, user profile & role permissions",
                    "No  — simple home page with a database query")
                .HighlightStyle(new Style(Color.Red)));

        // ── Confirm ──────────────────────────────────────────────────────────
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), projectName);

        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[grey]Option[/]");
        table.AddColumn("[white]Choice[/]");
        table.AddRow("Project",   $"[green]{projectName}[/]");
        table.AddRow("Output",    $"[grey]{outputDir}[/]");
        table.AddRow("API Style", apiStyle.Split(' ')[0]);
        table.AddRow("Database",  database.Split(' ')[0]);
        table.AddRow("Frontend",  frontend.Split(' ')[0]);
        table.AddRow("Auth",      authChoice.StartsWith("Yes") ? "[green]Yes[/]" : "[grey]No[/]");
        AnsiConsole.Write(table);

        if (!AnsiConsole.Confirm("\n[bold]Create project?[/]"))
        {
            AnsiConsole.MarkupLine("[yellow]Aborted.[/]");
            Environment.Exit(0);
        }

        return new ProjectOptions
        {
            ProjectName     = projectName,
            OutputDirectory = outputDir,
            ApiStyle        = apiStyle.StartsWith("MVC") ? ApiStyle.Mvc : ApiStyle.MinimalApi,
            Database        = database.StartsWith("PostgreSQL") ? Database.PostgreSQL
                            : database.StartsWith("SQL Server") ? Database.SqlServer
                            : Database.Sqlite,
            Frontend        = frontend.StartsWith("Vue")    ? Frontend.Vue
                            : frontend.StartsWith("React")  ? Frontend.React
                            : Frontend.Svelte,
            IncludeAuth     = authChoice.StartsWith("Yes"),
        };
    }
}
