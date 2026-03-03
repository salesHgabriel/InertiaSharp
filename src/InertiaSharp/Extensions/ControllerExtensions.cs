using Microsoft.AspNetCore.Mvc;

namespace InertiaSharp.Extensions;

/// <summary>
/// Extension methods on <see cref="ControllerBase"/> for rendering Inertia responses.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Returns an Inertia response for the given component.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="component">
    ///   Component path relative to your Pages directory (e.g. "Dashboard", "Auth/Login").
    /// </param>
    /// <param name="props">Props to pass to the component.</param>
    public static InertiaResult Inertia(
        this ControllerBase controller,
        string component,
        object? props = null)
    {
        var propsDictionary = props switch
        {
            null => new Dictionary<string, object?>(),
            IDictionary<string, object?> dict => dict,
            _ => props.GetType()
                      .GetProperties()
                      .ToDictionary(
                          p => char.ToLowerInvariant(p.Name[0]) + p.Name[1..],
                          p => p.GetValue(props))
        };

        return new InertiaResult(component, propsDictionary);
    }

    /// <summary>
    /// Returns an Inertia response with a fluent props builder.
    /// </summary>
    public static InertiaResult Inertia(
        this ControllerBase controller,
        string component,
        IDictionary<string, object?> props)
        => new(component, props);

    /// <summary>
    /// Redirects back with Inertia-compatible status code (303).
    /// </summary>
    public static RedirectResult InertiaBack(this Controller controller)
    {
        var referer = controller.Request.Headers["Referer"].FirstOrDefault()
                      ?? "/";
        controller.Response.StatusCode = 303;
        return controller.Redirect(referer);
    }
}
