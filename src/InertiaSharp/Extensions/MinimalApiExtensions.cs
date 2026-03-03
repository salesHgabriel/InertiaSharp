using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace InertiaSharp.Extensions;

/// <summary>
/// Extension methods for <see cref="IEndpointRouteBuilder"/> and
/// <see cref="RouteHandlerBuilder"/> that make Inertia + Minimal API
/// feel natural alongside standard ASP.NET Core patterns.
/// </summary>
public static class MinimalApiExtensions
{
    // ── Static page mapping ──────────────────────────────────────────────────

    /// <summary>
    /// Maps a GET route that always renders the same Inertia component
    /// with static (or no) props. Ideal for pages like About, Terms, etc.
    ///
    /// <code>
    /// app.MapInertia("/about", "Marketing/About");
    /// </code>
    /// </summary>
    public static RouteHandlerBuilder MapInertia(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        string component,
        object? props = null)
    {
        var staticProps = InertiaPageRenderer.ToProps(props);

        return endpoints.MapGet(pattern, () =>
            new InertiaHttpResult(component, staticProps));
    }

    // ── Group factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="RouteGroupBuilder"/> scoped to a URL prefix where
    /// every endpoint shares common Inertia metadata (auth, rate limiting, etc).
    ///
    /// <code>
    /// var auth = app.MapInertiaGroup("/app").RequireAuthorization();
    ///
    /// auth.MapGet("/dashboard", async (UserManager&lt;AppUser&gt; users, ClaimsPrincipal user) =>
    /// {
    ///     var appUser = await users.GetUserAsync(user);
    ///     return Results.Extensions.Inertia("Dashboard", new { name = appUser!.FullName });
    /// });
    /// </code>
    /// </summary>
    public static RouteGroupBuilder MapInertiaGroup(
        this IEndpointRouteBuilder endpoints,
        string prefix)
        => endpoints.MapGroup(prefix);

    // ── RouteHandlerBuilder decorators ───────────────────────────────────────

    /// <summary>
    /// Tags an Inertia endpoint so it appears correctly in Swagger / OpenAPI output.
    /// </summary>
    public static RouteHandlerBuilder WithInertiaMetadata(
        this RouteHandlerBuilder builder,
        string component,
        string? description = null)
    {
        builder
            .WithName(component.Replace('/', '_'))
            .WithDescription(description ?? $"Renders the Inertia component '{component}'.")
            .Produces(StatusCodes.Status200OK, contentType: "text/html")
            .Produces(StatusCodes.Status200OK, contentType: "application/json")
            .WithOpenApi();

        return builder;
    }

    /// <summary>
    /// Adds a display name to an endpoint, visible in logs and diagnostics.
    /// </summary>
    public static RouteHandlerBuilder WithInertiaDisplayName(
        this RouteHandlerBuilder builder,
        string component)
        => builder.WithDisplayName($"Inertia: {component}");
}
