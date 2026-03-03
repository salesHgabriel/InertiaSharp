using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InertiaSharp;

/// <summary>
/// An <see cref="IResult"/> for use in Minimal API endpoints.
///
/// Works identically to <see cref="InertiaResult"/> (the MVC version) but
/// implements the lighter-weight <c>IResult</c> interface instead of
/// <c>IActionResult</c>, so it can be returned directly from route handlers.
///
/// Usage in Minimal APIs:
/// <code>
/// app.MapGet("/dashboard", async (InertiaService inertia, ClaimsPrincipal user) =>
/// {
///     inertia.Share("auth", new { name = user.Identity!.Name });
///     return Results.Extensions.Inertia("Dashboard", new { message = "Hello!" });
/// }).RequireAuthorization();
/// </code>
/// </summary>
public sealed class InertiaHttpResult : IResult
{
    private readonly string _component;
    private readonly IDictionary<string, object?> _props;
    private readonly bool _encryptHistory;
    private readonly bool _clearHistory;

    public InertiaHttpResult(
        string component,
        IDictionary<string, object?> props,
        bool encryptHistory = false,
        bool clearHistory = false)
    {
        _component      = component;
        _props          = props;
        _encryptHistory = encryptHistory;
        _clearHistory   = clearHistory;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices
                                 .GetRequiredService<IOptions<InertiaOptions>>()
                                 .Value;

        return InertiaPageRenderer.ExecuteAsync(
            httpContext,
            _component,
            _props,
            options.RootView,
            _encryptHistory,
            _clearHistory);
    }
}
