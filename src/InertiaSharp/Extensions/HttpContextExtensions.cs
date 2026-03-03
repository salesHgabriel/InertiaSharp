using Microsoft.AspNetCore.Http;

namespace InertiaSharp.Extensions;

/// <summary>
/// 
/// </summary>
public static class HttpContextExtensions
{
    public static void AddFlashMessage(this HttpContext ctx, string key, string value) => ctx.Response.Cookies.Append(key, value);
    
    public static bool ExistFlashMessage(this HttpContext ctx, string key) => ctx.Request.Cookies[key] is not null;
    
    public static void RemoveFlashMessage(this HttpContext ctx, string key) => ctx.Response.Cookies.Delete(key);
    
    public static string? GetFlashMessage(this HttpContext ctx, string key) => ctx.Request.Cookies[key];
}