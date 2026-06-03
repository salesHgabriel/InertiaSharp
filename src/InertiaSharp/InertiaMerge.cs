namespace InertiaSharp;

/// <summary>
/// Wraps a prop value so the client merges it with existing data instead of replacing it
/// (Inertia.js v3+). Useful for infinite-scroll / polling scenarios where each partial
/// reload appends to an existing list rather than replacing the whole collection.
/// </summary>
/// <example>
/// <code>
/// return this.Inertia("Notifications", new
/// {
///     notifications = new InertiaMerge(GetLatestNotifications()),
/// });
/// </code>
/// </example>
public sealed class InertiaMerge(object? value)
{
    internal object? Value => value;
}
