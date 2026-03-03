using InertiaSharp.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InertiaSharp.Extensions;

/// <summary>
/// Dependency injection and middleware pipeline extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers InertiaSharp services. Call this in Program.cs:
    /// <code>
    /// builder.Services.AddInertia(options => {
    ///     options.RootView = "App";
    ///     options.Version  = "1.0.0";
    /// });
    /// </code>
    /// </summary>
    public static IServiceCollection AddInertia(
        this IServiceCollection services,
        Action<InertiaOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<InertiaOptions>(_ => { });

        // InertiaService is scoped so shared props are per-request
        services.AddScoped<InertiaService>();

        return services;
    }

    /// <summary>
    /// Adds the Inertia middleware to the pipeline. Place this after
    /// UseRouting() but before UseAuthentication() and MapControllers().
    /// </summary>
    public static IApplicationBuilder UseInertia(this IApplicationBuilder app)
        => app.UseMiddleware<InertiaMiddleware>();
}
