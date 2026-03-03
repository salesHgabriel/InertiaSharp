using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InertiaSharp;

/// <summary>
/// An <see cref="IActionResult"/> for use in MVC Controllers.
///
/// Renders either a full HTML page (first visit) or a JSON page object
/// (subsequent Inertia XHR visits). Delegates actual rendering to
/// <see cref="InertiaPageRenderer"/> — the same engine used by Minimal APIs.
///
/// Usage in a Controller:
/// <code>
/// public IActionResult Dashboard()
///     => this.Inertia("Dashboard", new { message = "Hello!" });
/// </code>
/// </summary>
public sealed class InertiaResult : IActionResult
{
    private readonly string _component;
    private readonly IDictionary<string, object?> _props;
    private readonly bool _encryptHistory;
    private readonly bool _clearHistory;

    public InertiaResult(
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

    public Task ExecuteResultAsync(ActionContext context)
    {
        var options = context.HttpContext.RequestServices
                             .GetRequiredService<IOptions<InertiaOptions>>()
                             .Value;

        return InertiaPageRenderer.ExecuteAsync(
            context.HttpContext,
            _component,
            _props,
            options.RootView,
            _encryptHistory,
            _clearHistory);
    }
}
