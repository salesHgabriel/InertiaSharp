using System.Text.Json.Serialization;

namespace InertiaSharp;

/// <summary>
/// Represents the Inertia page object returned to the client.
/// This is either embedded in the initial HTML as data-page or
/// returned as JSON for subsequent XHR requests.
/// </summary>
public class InertiaPage
{
    /// <summary>The frontend component name (e.g. "Auth/Login").</summary>
    [JsonPropertyName("component")]
    public string Component { get; set; } = default!;

    /// <summary>Props passed to the component.</summary>
    [JsonPropertyName("props")]
    public IDictionary<string, object?> Props { get; set; } = new Dictionary<string, object?>();

    /// <summary>Current URL of the page.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>Asset version hash used for cache-busting.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>Whether to encrypt the history entry.</summary>
    [JsonPropertyName("encryptHistory")]
    public bool EncryptHistory { get; set; }

    /// <summary>Whether to clear history on this navigation.</summary>
    [JsonPropertyName("clearHistory")]
    public bool ClearHistory { get; set; }

    /// <summary>
    /// Groups of prop keys that are deferred (Inertia.js v3+).
    /// Each inner list is a batch — the client fetches one batch per request.
    /// Props wrapped with <see cref="InertiaDeferred"/> are placed here and excluded
    /// from the initial <see cref="Props"/> payload.
    /// </summary>
    [JsonPropertyName("deferredProps")]
    public IList<IList<string>> DeferredProps { get; set; } = [];

    /// <summary>
    /// Prop keys whose values should be merged into existing client-side state
    /// on partial reloads, rather than replaced (Inertia.js v3+).
    /// Props wrapped with <see cref="InertiaMerge"/> are listed here.
    /// </summary>
    [JsonPropertyName("mergeProps")]
    public IList<string> MergeProps { get; set; } = [];
}
