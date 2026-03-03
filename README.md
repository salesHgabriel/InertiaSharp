# ⚡ InertiaSharp

> The **ASP.NET Core adapter for [Inertia.js](https://inertiajs.com)** — build modern SPAs with Vue, React, or Svelte without building a separate API. Inspired by [inertia-rails](https://github.com/inertiajs/inertia-rails).

---

## What is InertiaSharp?

InertiaSharp lets you build single-page applications using your **ASP.NET Core controllers** as the backend, while rendering your frontend in **Vue 3, React, or Svelte**. No REST API, no token auth, no separate frontend repo needed.

```
Browser → ASP.NET Core Controller → return this.Inertia("Dashboard", props)
                                              ↓
                               First visit: full HTML page
                               Subsequent: JSON page object (XHR)
                                              ↓
                                    Vue/React/Svelte renders
```

---

## How the Inertia Protocol Works

| Step | What happens |
|------|-------------|
| **First visit** | Browser makes a normal GET. Server returns full HTML with `<div id="app" data-page="{...}">`. Vite/Vue boots from `data-page`. |
| **Subsequent clicks** | Inertia intercepts `<Link>` clicks, sends XHR with `X-Inertia: true`. Server returns **JSON** page object instead of HTML. |
| **Asset version mismatch** | Server returns `409 Conflict` with `X-Inertia-Location`, client does a full reload to pick up new assets. |
| **Partial reloads** | Client sends `X-Inertia-Partial-Data` header. Server only includes requested props. |
| **POST redirects** | Middleware converts `302` → `303` so browsers re-GET instead of re-POST. |

---

## Package Structure

```
InertiaSharp/
├── src/
│   └── InertiaSharp/                   # 📦 NuGet Package
│       ├── InertiaOptions.cs           #   Configuration
│       ├── InertiaPage.cs              #   Page object model
│       ├── InertiaService.cs           #   Shared props (scoped)
│       ├── InertiaResult.cs            #   IActionResult implementation
│       ├── Middleware/
│       │   └── InertiaMiddleware.cs    #   Version check + 302→303
│       ├── Extensions/
│       │   ├── ControllerExtensions.cs #   this.Inertia(...)
│       │   └── ServiceCollectionExtensions.cs
│       └── TagHelpers/
│           └── InertiaTagHelper.cs     #   <inertia /> → <div id="app">
│
└── sample/
    └── InertiaSharp.Sample/            # 🎯 Demo app (ASP.NET Core 10)
        ├── Controllers/
        │   ├── AuthController.cs       #   Login, Register, Logout
        │   ├── DashboardController.cs  #   Protected dashboard
        │   └── ProfileController.cs   #   Profile + password change
        ├── Models/AppUser.cs           #   Extended Identity user
        ├── Data/AppDbContext.cs        #   EF Core + Identity
        ├── Permissions/               #   Roles + policy constants
        ├── Views/Shared/App.cshtml    #   Inertia shell view
        └── ClientApp/                 #   Vue 3 + Vite frontend
            └── src/
                ├── app.ts             #   Inertia + Vue bootstrap
                ├── Pages/
                │   ├── Auth/Login.vue
                │   ├── Auth/Register.vue
                │   ├── Dashboard.vue
                │   └── Profile/Edit.vue
                └── Layouts/
                    ├── AppLayout.vue  #   Sidebar for auth pages
                    └── GuestLayout.vue #  Centered card for auth
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org)

### 1. Install the NuGet package

```bash
dotnet add package InertiaSharp
```

### 2. Configure `Program.cs`

```csharp
using InertiaSharp.Extensions;

builder.Services.AddInertia(opt =>
{
    opt.RootView = "App";     // → Views/Shared/App.cshtml
    opt.Version  = "1.0.0";  // bump on asset changes
});

// ...
app.UseRouting();
app.UseInertia();             // ← after UseRouting, before UseAuth
app.UseAuthentication();
app.UseAuthorization();
```

### 3. Create the shell view `Views/Shared/App.cshtml`

```html
@addTagHelper *, InertiaSharp
<!DOCTYPE html>
<html>
<head>
    <meta name="csrf-token" content="@Antiforgery.GetAndStoreTokens(Context).RequestToken" />
    <script type="module" src="http://localhost:5173/src/app.ts"></script>
</head>
<body>
    <inertia />   <!-- renders: <div id="app" data-page="..."> -->
</body>
</html>
```

### 4. Return Inertia responses from controllers

```csharp
public class EventsController : Controller
{
    // GET /events
    public IActionResult Index()
        => this.Inertia("Events/Index", new { events = _db.Events.ToList() });

    // POST /events — validation errors sent back as props
    [HttpPost]
    public IActionResult Store([FromForm] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                k => k.Key,
                v => v.Value?.Errors.First().ErrorMessage ?? "");
            return this.Inertia("Events/Create", new { errors });
        }
        // ...
        return Redirect("/events");
    }
}
```

### 5. Shared props (middleware pattern)

```csharp
// In a base controller or middleware:
public class InertiaBaseController : Controller
{
    protected readonly InertiaService _inertia;

    public InertiaBaseController(InertiaService inertia)
    {
        _inertia = inertia;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _inertia.Share("auth", new
        {
            user = HttpContext.User.Identity?.IsAuthenticated == true
                ? new { name = HttpContext.User.Identity.Name }
                : null
        });
    }
}
```

---

## Development — Hot Reload

Run both servers simultaneously:

```bash
# From repo root:
./run-dev.sh
```

Or manually in two terminals:

```bash
# Terminal 1 — Vite dev server (HMR for .vue files)
cd sample/InertiaSharp.Sample/ClientApp
npm install
npm run dev

# Terminal 2 — ASP.NET Core (auto-restarts on C# changes)
cd sample/InertiaSharp.Sample
dotnet watch run
```

- **Vue changes** → instant hot module replacement (no page reload)
- **C# changes** → `dotnet watch` restarts the backend, Inertia triggers a full page reload

Visit: **https://localhost:5001**

Demo credentials: `admin@demo.com` / `Password123!`

---

## EF Core Migrations

```bash
cd sample/InertiaSharp.Sample

# Create initial migration
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Apply (also runs automatically on startup via MigrateAsync)
dotnet ef database update
```

---

## Permissions / Authorization Flow

```
User → Login → Cookie issued → Role assigned (Admin/Editor/Viewer)
                                     ↓
                         DashboardController.Index()
                         reads roles → passes `permissions` prop
                                     ↓
                         Dashboard.vue conditionally renders
                         sections based on permissions
```

Policy-based authorization on controllers:

```csharp
[Authorize(Policy = Policies.CanManageUsers)]
public IActionResult UserManagement() => this.Inertia("Admin/Users");
```

---

## Production Deployment

### Option A — Docker (recommended)

```bash
# Build image
docker build -t inertia-sharp-app .

# Run with persistent database volume
docker run -d \
  -p 8080:8080 \
  -v inertia_data:/data \
  --name inertia-app \
  inertia-sharp-app

# Visit http://localhost:8080
```

### Option B — `dotnet publish`

```bash
# Build frontend first
cd sample/InertiaSharp.Sample/ClientApp
npm ci && npm run build

# Publish .NET app
cd ..
dotnet publish -c Release -o ./publish

# Run
cd publish
./InertiaSharp.Sample
```

### Option C — Azure App Service / Railway / Fly.io

The Dockerfile works on any container platform. Set these environment variables:

| Variable | Value |
|---------|-------|
| `ConnectionStrings__Default` | Your connection string |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` |

### Asset versioning in production

`Program.cs` automatically computes the asset version from the Vite manifest:

```csharp
var viteManifestPath = Path.Combine(env.WebRootPath, ".vite", "manifest.json");
var assetVersion = File.Exists(viteManifestPath)
    ? Convert.ToHexString(MD5.HashData(File.ReadAllText(viteManifestPath)))
    : "1";

builder.Services.AddInertia(opt => { opt.Version = assetVersion; });
```

When you deploy a new frontend build, the version changes and all connected clients automatically reload to pick up new assets (409 Conflict flow).

---

## Minimal APIs

InertiaSharp supports **Minimal APIs** first-class via `InertiaHttpResult` (`IResult`) alongside the traditional MVC `InertiaResult` (`IActionResult`). Both share the exact same rendering engine — props merging, partial reloads, version checking — so you can mix and match freely or use Minimal APIs exclusively.

### Three Minimal API patterns

**Pattern 1 — `Results.Extensions.Inertia()`** — the standard idiomatic way:

```csharp
app.MapGet("/dashboard", async (UserManager<AppUser> users, ClaimsPrincipal user) =>
{
    var appUser = await users.GetUserAsync(user)!;
    return Results.Extensions.Inertia("Dashboard", new
    {
        name  = appUser.FullName,
        email = appUser.Email,
    });
})
.RequireAuthorization();
```

**Pattern 2 — `app.MapInertia()`** — zero-boilerplate static pages:

```csharp
// Component receives no dynamic props — perfect for About, Terms, 404, etc.
app.MapInertia("/about", "Marketing/About");
app.MapInertia("/403",   "Errors/Forbidden").AllowAnonymous();

// With static props
app.MapInertia("/features", "Marketing/Features", new
{
    plans = new[] { "Free", "Pro", "Enterprise" }
});
```

**Pattern 3 — `app.MapInertiaGroup()`** — grouped + authorized, with shared middleware:

```csharp
// All routes in the group inherit .RequireAuthorization()
// The endpoint filter runs on every request to share auth props
var authenticated = app
    .MapInertiaGroup("/app")
    .RequireAuthorization()
    .AddEndpointFilter(async (ctx, next) =>
    {
        var inertia = ctx.HttpContext.RequestServices.GetRequiredService<InertiaService>();
        inertia.Share("auth", new { user = ctx.HttpContext.User.Identity!.Name });
        return await next(ctx);
    });

authenticated.MapGet("dashboard", () =>
    Results.Extensions.Inertia("Dashboard"));

authenticated.MapGet("profile", () =>
    Results.Extensions.Inertia("Profile/Edit"));

// Fine-grained policy per route
authenticated
    .MapGet("admin/users", async (UserManager<AppUser> users) =>
        Results.Extensions.Inertia("Admin/Users", new { users = users.Users.ToList() }))
    .RequireAuthorization(Policies.CanManageUsers);
```

### Comparison: MVC vs Minimal API

| | MVC Controllers | Minimal APIs |
|---|---|---|
| Return type | `InertiaResult : IActionResult` | `InertiaHttpResult : IResult` |
| Syntax | `this.Inertia("Page", props)` | `Results.Extensions.Inertia("Page", props)` |
| Static pages | Custom action | `app.MapInertia(route, component)` |
| Grouped routes | Base controller + `[Route]` | `app.MapInertiaGroup(prefix)` |
| Shared props | Inject `InertiaService` | Same — inject `InertiaService` |
| Rendering engine | `InertiaPageRenderer` ✓ | `InertiaPageRenderer` ✓ (shared) |

### Running the Minimal API sample

```bash
# Minimal API sample runs on port 5002
./run-dev.sh minimal

# Visit https://localhost:5002
# Credentials: admin@demo.com / Password123!
```

---

## Using React or Svelte instead of Vue

The **server-side package is framework-agnostic**. Only the client bootstrap changes.

**React:**
```bash
npm install @inertiajs/react react react-dom
```
```tsx
// src/app.tsx
import { createInertiaApp } from '@inertiajs/react'
import { createRoot } from 'react-dom/client'

createInertiaApp({
  resolve: name => {
    const pages = import.meta.glob('./Pages/**/*.tsx', { eager: true })
    return pages[`./Pages/${name}.tsx`]
  },
  setup({ el, App, props }) {
    createRoot(el).render(<App {...props} />)
  },
})
```

**Svelte:**
```bash
npm install @inertiajs/svelte svelte
```
```ts
// src/app.ts
import { createInertiaApp } from '@inertiajs/svelte'
createInertiaApp({
  resolve: name => import(`./Pages/${name}.svelte`),
  setup({ el, App, props }) {
    new App({ target: el, props })
  },
})
```

---

## License

MIT
