using Microsoft.AspNetCore.Http;

namespace InertiaSharp.Extensions;

/// <summary>
/// Extends <see cref="IResultExtensions"/> so Inertia responses can be returned
/// via the idiomatic <c>Results.Extensions.Inertia()</c> syntax in Minimal APIs.
///
/// <example>
/// <code>
/// app.MapGet("/dashboard", (ClaimsPrincipal user) =>
///     Results.Extensions.Inertia("Dashboard", new { name = user.Identity!.Name }));
/// </code>
/// </example>
/// </summary>
public static class InertiaResultExtensions
{
    /// <summary>
    /// Returns an Inertia page response.
    /// </summary>
    /// <param name="_">The <see cref="IResultExtensions"/> instance (unused — enables the extension syntax).</param>
    /// <param name="component">
    ///   The frontend component name, relative to your Pages directory.
    ///   Examples: <c>"Dashboard"</c>, <c>"Auth/Login"</c>, <c>"Admin/Users/Index"</c>.
    /// </param>
    /// <param name="props">
    ///   Props to pass to the component. Accepts anonymous objects, dictionaries, or records.
    ///   Keys are automatically converted to camelCase.
    /// </param>
    public static IResult Inertia(
        this IResultExtensions _,
        string component,
        object? props = null)
        => new InertiaHttpResult(component, InertiaPageRenderer.ToProps(props));

    /// <summary>
    /// Returns an Inertia page response with an explicit props dictionary.
    /// </summary>
    public static IResult Inertia(
        this IResultExtensions _,
        string component,
        IDictionary<string, object?> props)
        => new InertiaHttpResult(component, props);

    /// <summary>
    /// Returns an Inertia page response with history encryption enabled.
    /// Useful for sensitive pages (e.g. checkout, medical records).
    /// </summary>
    public static IResult InertiaEncrypted(
        this IResultExtensions _,
        string component,
        object? props = null)
        => new InertiaHttpResult(component, InertiaPageRenderer.ToProps(props), encryptHistory: true);
}
