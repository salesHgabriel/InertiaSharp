using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using InertiaSharp.Middleware;

namespace InertiaSharp.Test;

public class InertiaMiddlewareTests
{
    private static InertiaMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new InertiaMiddleware(next);
    }

    private static IOptions<InertiaOptions> CreateOptions(string? version = null)
        => Options.Create(new InertiaOptions { Version = version });

    // ── Non-Inertia requests ──────────────────────────────────────────────────

    [Fact]
    public async Task NonInertiaRequest_PassesThrough()
    {
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";

        await middleware.InvokeAsync(context, CreateOptions("1.0"));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task NonInertiaRequest_PostWith302_Stays302()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(302, context.Response.StatusCode);
    }

    // ── Asset version check ───────────────────────────────────────────────────

    [Fact]
    public async Task InertiaRequest_VersionMatch_PassesThrough()
    {
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";
        context.Request.Headers["X-Inertia-Version"] = "1.0";

        await middleware.InvokeAsync(context, CreateOptions("1.0"));

        Assert.True(nextCalled);
        Assert.NotEqual(409, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_VersionMismatch_OnGet_Returns409()
    {
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";
        context.Request.Headers["X-Inertia-Version"] = "old-version";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/dashboard";

        await middleware.InvokeAsync(context, CreateOptions("new-version"));

        Assert.Equal(409, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_VersionMismatch_SetsLocationHeader()
    {
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";
        context.Request.Headers["X-Inertia-Version"] = "old";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/page";

        await middleware.InvokeAsync(context, CreateOptions("new"));

        Assert.Equal("https://example.com/page", context.Response.Headers["X-Inertia-Location"].ToString());
    }

    [Fact]
    public async Task InertiaRequest_VersionMismatch_SetsLocationHeaderWithQueryString()
    {
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";
        context.Request.Headers["X-Inertia-Version"] = "old";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/search";
        context.Request.QueryString = new QueryString("?q=test");

        await middleware.InvokeAsync(context, CreateOptions("new"));

        Assert.Equal("https://example.com/search?q=test", context.Response.Headers["X-Inertia-Location"].ToString());
    }

    [Fact]
    public async Task InertiaRequest_VersionMismatch_OnPost_DoesNotReturn409()
    {
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Headers["X-Inertia"] = "true";
        context.Request.Headers["X-Inertia-Version"] = "old";

        await middleware.InvokeAsync(context, CreateOptions("new"));

        Assert.True(nextCalled);
        Assert.NotEqual(409, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_NoServerVersion_PassesThrough()
    {
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";
        context.Request.Headers["X-Inertia-Version"] = "1.0";

        await middleware.InvokeAsync(context, CreateOptions(null));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InertiaRequest_NoClientVersion_PassesThrough()
    {
        bool nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";
        // No X-Inertia-Version header

        await middleware.InvokeAsync(context, CreateOptions("1.0"));

        Assert.True(nextCalled);
    }

    // ── 302 → 303 conversion ──────────────────────────────────────────────────

    [Fact]
    public async Task InertiaRequest_PostWith302_ConvertedTo303()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(303, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_PutWith302_ConvertedTo303()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "PUT";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(303, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_PatchWith302_ConvertedTo303()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "PATCH";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(303, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_DeleteWith302_ConvertedTo303()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "DELETE";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(303, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_GetWith302_Stays302()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(302, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_HeadWith302_Stays302()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 302; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "HEAD";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(302, context.Response.StatusCode);
    }

    [Fact]
    public async Task InertiaRequest_PostWithNon302_NotChanged()
    {
        var middleware = CreateMiddleware(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Headers["X-Inertia"] = "true";

        await middleware.InvokeAsync(context, CreateOptions());

        Assert.Equal(200, context.Response.StatusCode);
    }
}
