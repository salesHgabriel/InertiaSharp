namespace InertiaSharp;

/// <summary>
/// Wraps a prop factory for deferred loading (Inertia.js v3+).
/// Deferred props are excluded from the initial page response and loaded
/// by the client via an automatic follow-up partial reload request.
/// </summary>
/// <example>
/// <code>
/// return this.Inertia("Dashboard", new
/// {
///     user    = GetUser(),                                      // eager — included on first load
///     reports = new InertiaDeferred(() => GetHeavyReports()),  // lazy  — loaded after mount
/// });
/// </code>
/// </example>
public sealed class InertiaDeferred
{
    private readonly Func<object?> _factory;

    public InertiaDeferred(Func<object?> factory) => _factory = factory;

    internal object? Resolve() => _factory();
}
