namespace InertiaSharp;

/// <summary>
/// Scoped service that holds Inertia state for the current request:
/// shared props, flash data, and lazy prop factories.
/// Inject this into controllers or middleware to share global data.
/// </summary>
public class InertiaService
{
    private readonly Dictionary<string, Func<object?>> _sharedProps = new();
    private readonly Dictionary<string, object?> _flashProps = new();

    // ── Shared props ────────────────────────────────────────────────────────

    /// <summary>
    /// Share a static value that will be merged into every Inertia response.
    /// </summary>
    public InertiaService Share(string key, object? value)
    {
        _sharedProps[key] = () => value;
        return this;
    }

    /// <summary>
    /// Share a lazily-evaluated value (factory is called once per request).
    /// </summary>
    public InertiaService Share(string key, Func<object?> factory)
    {
        _sharedProps[key] = factory;
        return this;
    }

    /// <summary>
    /// Share multiple values at once.
    /// </summary>
    public InertiaService Share(IDictionary<string, object?> props)
    {
        foreach (var (k, v) in props)
            _sharedProps[k] = () => v;
        return this;
    }

    // ── Flash props ─────────────────────────────────────────────────────────

    /// <summary>
    /// Add a one-time flash value (typically from TempData).
    /// </summary>
    public InertiaService Flash(string key, object? value)
    {
        _flashProps[key] = value;
        return this;
    }

    // ── Resolution ──────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves all shared + flash props into a merged dictionary.
    /// </summary>
    internal IDictionary<string, object?> ResolveSharedProps()
    {
        var result = new Dictionary<string, object?>();

        foreach (var (key, factory) in _sharedProps)
            result[key] = factory();

        foreach (var (key, value) in _flashProps)
            result[key] = value;

        return result;
    }
}
