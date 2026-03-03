using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InertiaSharp.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

/// <summary>
/// Custom extension to run both projects frontend and backend
/// npm run dev
/// </summary>
public static class ViteDevelopmentExtensions
{
    /// <summary>
    /// Inicia o Vite dev server automaticamente quando em Development.
    /// put code before app.Run():
    ///   app.UseViteDevelopmentServer();
    /// </summary>
    public static IApplicationBuilder UseViteDevelopmentServer(
        this IApplicationBuilder app,
        string workingDirectory = "ClientApp",
        string script = "run dev")
    {
        var env = app.ApplicationServices
            .GetRequiredService<IWebHostEnvironment>();

        if (!env.IsDevelopment())
            return app;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), workingDirectory);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException(
                $"UseViteDevelopmentServer: directory '{fullPath}' not found.");

        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName         = OperatingSystem.IsWindows() ? "cmd" : "npm",
                Arguments        = OperatingSystem.IsWindows() ? $"/c npm {script}" : script,
                WorkingDirectory = fullPath,
                UseShellExecute  = false,
            }
        };

        process.Start();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        };

        return app;
    }
}