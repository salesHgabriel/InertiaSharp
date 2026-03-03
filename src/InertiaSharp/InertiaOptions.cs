namespace InertiaSharp;

/// <summary>
/// Configuration options for the InertiaSharp adapter.
/// </summary>
public class InertiaOptions
{
    /// <summary>
    /// The root Razor view that renders the Inertia app shell.
    /// Defaults to "App" (Views/Shared/App.cshtml).
    /// </summary>
    public string RootView { get; set; } = "App";

    /// <summary>
    /// The current asset version. When this changes, Inertia triggers a full
    /// page reload on the client. Typically set to a file hash or build timestamp.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// When true, CSRF tokens are automatically shared as props.
    /// </summary>
    public bool SsrEnabled { get; set; } = false;

    /// <summary>
    /// Optional SSR server URL (e.g. http://localhost:13714).
    /// </summary>
    public string? SsrUrl { get; set; }
}
