using Microsoft.AspNetCore.Http;

namespace InertiaSharp.Test;

public class InertiaPageRendererTests
{
    // ── ToProps ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToProps_Null_ReturnsEmptyDictionary()
    {
        var result = InertiaPageRenderer.ToProps(null);
        Assert.Empty(result);
    }

    [Fact]
    public void ToProps_ExistingDictionary_ReturnsSameInstance()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "value" };
        var result = InertiaPageRenderer.ToProps(dict);
        Assert.Same(dict, result);
    }

    [Fact]
    public void ToProps_AnonymousObject_ConvertsToCamelCase()
    {
        var result = InertiaPageRenderer.ToProps(new { FirstName = "John", LastName = "Doe", Age = 30 });

        Assert.True(result.ContainsKey("firstName"));
        Assert.True(result.ContainsKey("lastName"));
        Assert.True(result.ContainsKey("age"));
        Assert.Equal("John", result["firstName"]);
        Assert.Equal("Doe", result["lastName"]);
        Assert.Equal(30, result["age"]);
    }

    [Fact]
    public void ToProps_SingleUppercaseProp_LowercasesFirstChar()
    {
        var result = InertiaPageRenderer.ToProps(new { Name = "test" });
        Assert.True(result.ContainsKey("name"));
        Assert.Equal("test", result["name"]);
    }

    [Fact]
    public void ToProps_AlreadyCamelCase_KeysUnchanged()
    {
        var result = InertiaPageRenderer.ToProps(new { name = "test", userId = 42 });
        Assert.True(result.ContainsKey("name"));
        Assert.True(result.ContainsKey("userId"));
    }

    [Fact]
    public void ToProps_NullPropValue_Preserved()
    {
        var result = InertiaPageRenderer.ToProps(new { Value = (string?)null });
        Assert.True(result.ContainsKey("value"));
        Assert.Null(result["value"]);
    }

    // ── BuildPage ─────────────────────────────────────────────────────────────

    private static (HttpRequest request, InertiaService service, InertiaOptions options) CreateDefaults(
        string path = "/",
        string? queryString = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        if (queryString is not null)
            ctx.Request.QueryString = new QueryString(queryString);

        return (ctx.Request, new InertiaService(), new InertiaOptions());
    }

    [Fact]
    public void BuildPage_SetsComponent()
    {
        var (req, svc, opt) = CreateDefaults();
        var page = InertiaPageRenderer.BuildPage(req, "Auth/Login", new Dictionary<string, object?>(), svc, opt);
        Assert.Equal("Auth/Login", page.Component);
    }

    [Fact]
    public void BuildPage_SetsUrlFromRequestPath()
    {
        var (req, svc, opt) = CreateDefaults("/dashboard");
        var page = InertiaPageRenderer.BuildPage(req, "Dashboard", new Dictionary<string, object?>(), svc, opt);
        Assert.Equal("/dashboard", page.Url);
    }

    [Fact]
    public void BuildPage_SetsUrlWithQueryString()
    {
        var (req, svc, opt) = CreateDefaults("/users", "?page=2");
        var page = InertiaPageRenderer.BuildPage(req, "Users", new Dictionary<string, object?>(), svc, opt);
        Assert.Equal("/users?page=2", page.Url);
    }

    [Fact]
    public void BuildPage_SetsVersionFromOptions()
    {
        var (req, svc, opt) = CreateDefaults();
        opt.Version = "abc123";
        var page = InertiaPageRenderer.BuildPage(req, "Home", new Dictionary<string, object?>(), svc, opt);
        Assert.Equal("abc123", page.Version);
    }

    [Fact]
    public void BuildPage_NullVersion_WhenNotConfigured()
    {
        var (req, svc, opt) = CreateDefaults();
        var page = InertiaPageRenderer.BuildPage(req, "Home", new Dictionary<string, object?>(), svc, opt);
        Assert.Null(page.Version);
    }

    [Fact]
    public void BuildPage_MergesSharedPropsAndComponentProps()
    {
        var (req, svc, opt) = CreateDefaults("/dashboard");
        svc.Share("user", "John");

        var componentProps = new Dictionary<string, object?> { ["title"] = "Dashboard" };
        var page = InertiaPageRenderer.BuildPage(req, "Dashboard", componentProps, svc, opt);

        Assert.Equal("John", page.Props["user"]);
        Assert.Equal("Dashboard", page.Props["title"]);
    }

    [Fact]
    public void BuildPage_ComponentProps_OverrideSharedPropsWithSameKey()
    {
        var (req, svc, opt) = CreateDefaults();
        svc.Share("title", "Shared Title");

        var componentProps = new Dictionary<string, object?> { ["title"] = "Component Title" };
        var page = InertiaPageRenderer.BuildPage(req, "Home", componentProps, svc, opt);

        Assert.Equal("Component Title", page.Props["title"]);
    }

    [Fact]
    public void BuildPage_EncryptHistory_SetsFlagOnPage()
    {
        var (req, svc, opt) = CreateDefaults();
        var page = InertiaPageRenderer.BuildPage(req, "Secure", new Dictionary<string, object?>(), svc, opt, encryptHistory: true);
        Assert.True(page.EncryptHistory);
    }

    [Fact]
    public void BuildPage_ClearHistory_SetsFlagOnPage()
    {
        var (req, svc, opt) = CreateDefaults();
        var page = InertiaPageRenderer.BuildPage(req, "Home", new Dictionary<string, object?>(), svc, opt, clearHistory: true);
        Assert.True(page.ClearHistory);
    }

    [Fact]
    public void BuildPage_DefaultFlags_AreFalse()
    {
        var (req, svc, opt) = CreateDefaults();
        var page = InertiaPageRenderer.BuildPage(req, "Home", new Dictionary<string, object?>(), svc, opt);
        Assert.False(page.EncryptHistory);
        Assert.False(page.ClearHistory);
    }

    // ── Partial reloads ───────────────────────────────────────────────────────

    [Fact]
    public void BuildPage_PartialReload_OnlyIncludesRequestedProps()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Component"] = "Dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Data"] = "title";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"] = "John",
            ["stats"] = new { count = 10 },
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.False(page.Props.ContainsKey("user"));
        Assert.False(page.Props.ContainsKey("stats"));
    }

    [Fact]
    public void BuildPage_PartialReload_AlwaysIncludesErrors()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Component"] = "Dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Data"] = "title";

        var componentProps = new Dictionary<string, object?> { ["title"] = "Dashboard" };
        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("errors"));
    }

    [Fact]
    public void BuildPage_PartialReload_MultipleRequestedKeys()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Component"] = "Dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Data"] = "title,user";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"] = "John",
            ["stats"] = new { count = 10 },
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.True(page.Props.ContainsKey("user"));
        Assert.False(page.Props.ContainsKey("stats"));
    }

    [Fact]
    public void BuildPage_PartialReload_DifferentComponent_IncludesAllProps()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Component"] = "OtherComponent";
        ctx.Request.Headers["X-Inertia-Partial-Data"] = "title";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"] = "John",
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.True(page.Props.ContainsKey("user"));
    }

    [Fact]
    public void BuildPage_PartialReload_MissingDataHeader_IncludesAllProps()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Component"] = "Dashboard";
        // No X-Inertia-Partial-Data header

        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"] = "John",
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.True(page.Props.ContainsKey("user"));
    }

    // ── Except header ─────────────────────────────────────────────────────────

    [Fact]
    public void BuildPage_ExceptHeader_ExcludesSpecifiedProp()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Except"] = "heavyData";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["heavyData"] = new { big = "object" },
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.False(page.Props.ContainsKey("heavyData"));
    }

    [Fact]
    public void BuildPage_ExceptHeader_ExcludesMultipleProps()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Except"] = "heavyData,metrics";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["heavyData"] = "big",
            ["metrics"] = "lots",
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.False(page.Props.ContainsKey("heavyData"));
        Assert.False(page.Props.ContainsKey("metrics"));
    }
}
