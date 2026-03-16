---
name: InertiaSharp CLI Templates
description: Template CLI tool created for InertiaSharp in the templates/ directory
type: project
---

A `dotnet tool` CLI was created at `templates/src/InertiaSharp.Cli/` that scaffolds InertiaSharp starter projects interactively (similar to Laravel installer).

**Why:** User wanted a CLI wizard like Laravel new to scaffold complete starter kits.

**How to apply:** When working on the CLI, it lives in `templates/` as a separate solution (`InertiaSharp.Templates.sln`).

## Key design decisions:
- Command: `inertiasharp [project-name]`
- Uses Spectre.Console for interactive prompts
- Generates files programmatically (not dotnet new templates)
- Frontend generator namespaces use `InertiaSharp.Cli.Generators.Views` (not `.Frontend`) to avoid conflict with the `Frontend` enum in Models

## Wizard flow:
1. Project name
2. API style: MVC Controllers | Minimal API
3. Database: SQLite | PostgreSQL | SQL Server
4. Frontend: Vue 3 + Reka UI | React + shadcn/ui | Svelte + shadcn-svelte
5. Auth: Identity (full auth system) | No (simple home + DB query)

## File structure:
- `Models/ProjectOptions.cs` - Options model with enums
- `Wizard/ProjectWizard.cs` - Interactive Spectre.Console prompts
- `Generators/ProjectGenerator.cs` - Main orchestrator
- `Generators/Backend/` - CsprojGenerator, SharedBackendGenerator, MvcGenerator, MinimalApiGenerator
- `Generators/Frontend/` - VueGenerator, ReactGenerator, SvelteGenerator (namespace: Views)

## Important notes:
- Vue templates use `"""` raw strings (not `$$"""`) to avoid conflict with Vue's `{{ }}` double-brace syntax and C# interpolation
- For Vue templates that need `opts.ProjectName`: use `.Replace("__PROJECT_NAME__", opts.ProjectName)`
- React/Svelte use `$$"""` safely since JSX/Svelte use single braces `{expr}`
- Auth uses ASP.NET Core Identity; no-auth uses simple SampleItem model
