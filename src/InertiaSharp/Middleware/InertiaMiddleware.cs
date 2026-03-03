using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace InertiaSharp.Middleware;

/// <summary>
/// Inertia middleware that:
/// 1. Detects asset version mismatches and returns 409 Conflict.
/// 2. Converts 302 redirects to 303 See Other after non-GET requests
///    (so browsers re-GET instead of replaying the body).
/// 3. Prevents full-page reloads from serving stale cached JSON.
/// </summary>
public class InertiaMiddleware
{
    private readonly RequestDelegate _next;

    public InertiaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<InertiaOptions> options)
    {
        var request = context.Request;
        var response = context.Response;
        var isInertiaRequest = request.Headers.ContainsKey("X-Inertia");

        if (isInertiaRequest)
        {
            // ── Asset version check ─────────────────────────────────────────
            var clientVersion = request.Headers["X-Inertia-Version"].FirstOrDefault();
            var serverVersion = options.Value.Version;

            if (!string.IsNullOrEmpty(serverVersion) &&
                !string.IsNullOrEmpty(clientVersion) &&
                clientVersion != serverVersion &&
                HttpMethods.IsGet(request.Method))
            {
                response.Headers["X-Inertia-Location"] =
                    $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                response.StatusCode = StatusCodes.Status409Conflict;
                return;
            }
        }

        await _next(context);

        if (isInertiaRequest)
        {
            // ── 302 → 303 for non-GET requests ──────────────────────────────
            if (response.StatusCode == StatusCodes.Status302Found &&
                !HttpMethods.IsGet(request.Method) &&
                !HttpMethods.IsHead(request.Method))
            {
                response.StatusCode = StatusCodes.Status303SeeOther;
            }
        }
    }
}
