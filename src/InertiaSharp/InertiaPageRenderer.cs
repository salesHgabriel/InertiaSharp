using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace InertiaSharp;

/// <summary>
/// Core Inertia rendering logic shared between:
/// - <see cref="InertiaResult"/> (MVC / Controllers)
/// - <see cref="InertiaHttpResult"/> (Minimal APIs)
///
/// Handles prop merging, partial reloads, and dispatching between
/// a full HTML response (first visit) and a JSON response (XHR).
/// </summary>
internal static class InertiaPageRenderer
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    // ── Build the page object ────────────────────────────────────────────────

    internal static InertiaPage BuildPage(
        HttpRequest request,
        string component,
        IDictionary<string, object?> componentProps,
        InertiaService inertia,
        InertiaOptions options,
        bool encryptHistory = false,
        bool clearHistory = false)
    {
        // Start from shared props, then layer component-specific props on top
        var sharedProps = inertia.ResolveSharedProps();
        var mergedProps = new Dictionary<string, object?>(sharedProps);

        var partialComponent = request.Headers["X-Inertia-Partial-Component"].FirstOrDefault();
        var partialData      = request.Headers["X-Inertia-Partial-Data"].FirstOrDefault();
        var partialExcept    = request.Headers["X-Inertia-Partial-Except"].FirstOrDefault();

        var isPartialReload =
            !string.IsNullOrEmpty(partialComponent) &&
            partialComponent == component &&
            !string.IsNullOrEmpty(partialData);

        if (isPartialReload)
        {
            // Only include explicitly requested props
            var onlyKeys = partialData!
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (k, v) in componentProps)
                if (onlyKeys.Contains(k))
                    mergedProps[k] = v;

            // errors are always included so form state isn't lost
            if (!mergedProps.ContainsKey("errors"))
                mergedProps["errors"] = new Dictionary<string, string>();
        }
        else
        {
            var exceptKeys = string.IsNullOrEmpty(partialExcept)
                ? new HashSet<string>()
                : partialExcept.Split(',')
                               .Select(k => k.Trim())
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (k, v) in componentProps)
                if (!exceptKeys.Contains(k))
                    mergedProps[k] = v;
        }

        return new InertiaPage
        {
            Component      = component,
            Props          = mergedProps,
            Url            = $"{request.Path}{request.QueryString}",
            Version        = options.Version,
            EncryptHistory = encryptHistory,
            ClearHistory   = clearHistory,
        };
    }

    // ── Dispatch the response ────────────────────────────────────────────────

    /// <summary>
    /// Writes the Inertia response to <paramref name="httpContext"/>.
    /// Called by both MVC and Minimal API result implementations.
    /// </summary>
    internal static async Task ExecuteAsync(
        HttpContext httpContext,
        string component,
        IDictionary<string, object?> componentProps,
        string rootViewName,
        bool encryptHistory = false,
        bool clearHistory = false)
    {
        var request  = httpContext.Request;
        var response = httpContext.Response;

        var inertia = httpContext.RequestServices.GetRequiredService<InertiaService>();
        var options = httpContext.RequestServices
                                 .GetRequiredService<Microsoft.Extensions.Options.IOptions<InertiaOptions>>()
                                 .Value;

        var page    = BuildPage(request, component, componentProps, inertia, options, encryptHistory, clearHistory);
        var isInertia = request.Headers.ContainsKey("X-Inertia");

        response.Headers["Vary"] = "X-Inertia";

        if (isInertia)
        {
            // ── XHR: return the JSON page object directly ──────────────────
            response.Headers["X-Inertia"] = "true";
            response.ContentType  = "application/json";
            response.StatusCode   = StatusCodes.Status200OK;

            await response.WriteAsync(JsonSerializer.Serialize(page, JsonOptions));
        }
        else
        {
            // ── First visit: render the Razor shell view ───────────────────
            var pageJson = JsonSerializer.Serialize(page, JsonOptions);
            await RenderRazorViewAsync(httpContext, rootViewName, pageJson);
        }
    }

    // ── Razor view rendering (works in both MVC and Minimal API contexts) ───

    private static async Task RenderRazorViewAsync(
        HttpContext httpContext,
        string viewName,
        string pageJson)
    {
        var razorEngine    = httpContext.RequestServices.GetRequiredService<IRazorViewEngine>();
        var tempDataProvider = httpContext.RequestServices.GetRequiredService<ITempDataProvider>();

        // Build a minimal ActionContext — Razor requires one even in Minimal APIs
        var routeData     = httpContext.GetRouteData() ?? new RouteData();
        var actionContext = new ActionContext(httpContext, routeData, new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        var findResult = razorEngine.FindView(actionContext, viewName, isMainPage: true);

        if (!findResult.Success)
        {
            var searchedLocations = string.Join(", ", findResult.SearchedLocations ?? []);
            throw new InvalidOperationException(
                $"InertiaSharp: Razor view '{viewName}' not found. " +
                $"Searched: {searchedLocations}. " +
                $"Create Views/Shared/{viewName}.cshtml with <inertia /> inside.");
        }

        var view     = findResult.View;
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            ["InertiaPage"] = pageJson,
        };
        var tempData = new TempDataDictionary(httpContext, tempDataProvider);

        httpContext.Response.ContentType = "text/html; charset=utf-8";

        await using var writer = new StreamWriter(httpContext.Response.Body, leaveOpen: true);
        var viewContext = new ViewContext(
            actionContext,
            view,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);
        await writer.FlushAsync();
    }

    // ── Prop dictionary helpers ──────────────────────────────────────────────

    /// <summary>
    /// Converts an anonymous object or existing dictionary to
    /// <c>IDictionary&lt;string, object?&gt;</c> with camelCase keys.
    /// </summary>
    internal static IDictionary<string, object?> ToProps(object? props) =>
        props switch
        {
            null => new Dictionary<string, object?>(),
            IDictionary<string, object?> dict => dict,
            _ => props.GetType()
                      .GetProperties()
                      .ToDictionary(
                          p => char.ToLowerInvariant(p.Name[0]) + p.Name[1..],
                          p => p.GetValue(props))
        };
}
