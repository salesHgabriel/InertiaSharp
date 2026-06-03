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

    // ── BuildPage helpers ─────────────────────────────────────────────────────

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

    private static DefaultHttpContext PartialReloadContext(
        string path,
        string component,
        string partialData,
        string? partialExcept = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        ctx.Request.Headers["X-Inertia-Partial-Component"] = component;
        ctx.Request.Headers["X-Inertia-Partial-Data"] = partialData;
        if (partialExcept is not null)
            ctx.Request.Headers["X-Inertia-Partial-Except"] = partialExcept;
        return ctx;
    }

    // ── BuildPage — basic ─────────────────────────────────────────────────────

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

    [Fact]
    public void BuildPage_NoSpecialProps_DeferredAndMergeListsAreEmpty()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?> { ["title"] = "Hello" };
        var page = InertiaPageRenderer.BuildPage(req, "Home", props, svc, opt);

        Assert.Empty(page.DeferredProps);
        Assert.Empty(page.MergeProps);
    }

    // ── BuildPage — deferred props (v3) ───────────────────────────────────────

    [Fact]
    public void BuildPage_DeferredProp_IsExcludedFromProps()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["user"]    = "John",
            ["reports"] = new InertiaDeferred(() => new[] { "report1" }),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Dashboard", props, svc, opt);

        Assert.True(page.Props.ContainsKey("user"));
        Assert.False(page.Props.ContainsKey("reports"));
    }

    [Fact]
    public void BuildPage_DeferredProp_AddedToDeferredPropsAsOwnGroup()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["reports"] = new InertiaDeferred(() => "data"),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Dashboard", props, svc, opt);

        Assert.Single(page.DeferredProps);
        Assert.Single(page.DeferredProps[0]);
        Assert.Equal("reports", page.DeferredProps[0][0]);
    }

    [Fact]
    public void BuildPage_MultipleDeferredProps_EachInOwnGroup()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["a"] = new InertiaDeferred(() => 1),
            ["b"] = new InertiaDeferred(() => 2),
            ["c"] = new InertiaDeferred(() => 3),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Dashboard", props, svc, opt);

        Assert.Equal(3, page.DeferredProps.Count);
        // each group has exactly one key
        Assert.All(page.DeferredProps, group => Assert.Single(group));
        var keys = page.DeferredProps.SelectMany(g => g).ToHashSet();
        Assert.Contains("a", keys);
        Assert.Contains("b", keys);
        Assert.Contains("c", keys);
    }

    [Fact]
    public void BuildPage_DeferredProp_FactoryNotCalledOnInitialRender()
    {
        var (req, svc, opt) = CreateDefaults();
        int callCount = 0;
        var props = new Dictionary<string, object?>
        {
            ["data"] = new InertiaDeferred(() => { callCount++; return "value"; }),
        };

        InertiaPageRenderer.BuildPage(req, "Page", props, svc, opt);

        Assert.Equal(0, callCount);
    }

    // ── BuildPage — merge props (v3) ──────────────────────────────────────────

    [Fact]
    public void BuildPage_MergeProp_IsIncludedInProps()
    {
        var (req, svc, opt) = CreateDefaults();
        var items = new[] { "item1", "item2" };
        var props = new Dictionary<string, object?>
        {
            ["feed"] = new InertiaMerge(items),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Feed", props, svc, opt);

        Assert.True(page.Props.ContainsKey("feed"));
        Assert.Same(items, page.Props["feed"]);
    }

    [Fact]
    public void BuildPage_MergeProp_KeyAddedToMergeProps()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["feed"] = new InertiaMerge(new[] { "a", "b" }),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Feed", props, svc, opt);

        Assert.Single(page.MergeProps);
        Assert.Equal("feed", page.MergeProps[0]);
    }

    [Fact]
    public void BuildPage_MultipleMergeProps_AllKeysInMergeProps()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["posts"]    = new InertiaMerge(new[] { "p1" }),
            ["comments"] = new InertiaMerge(new[] { "c1" }),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Feed", props, svc, opt);

        Assert.Equal(2, page.MergeProps.Count);
        Assert.Contains("posts", page.MergeProps);
        Assert.Contains("comments", page.MergeProps);
    }

    [Fact]
    public void BuildPage_MergeProp_NotAddedToDeferredProps()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["feed"] = new InertiaMerge("data"),
        };

        var page = InertiaPageRenderer.BuildPage(req, "Feed", props, svc, opt);

        Assert.Empty(page.DeferredProps);
    }

    [Fact]
    public void BuildPage_RegularProp_NotAddedToMergeOrDeferredProps()
    {
        var (req, svc, opt) = CreateDefaults();
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello",
        };

        var page = InertiaPageRenderer.BuildPage(req, "Home", props, svc, opt);

        Assert.Empty(page.DeferredProps);
        Assert.Empty(page.MergeProps);
    }

    // ── BuildPage — partial reloads ───────────────────────────────────────────

    [Fact]
    public void BuildPage_PartialReload_OnlyIncludesRequestedProps()
    {
        var ctx = PartialReloadContext("/dashboard", "Dashboard", "title");
        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"]  = "John",
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
        var ctx = PartialReloadContext("/dashboard", "Dashboard", "title");
        var componentProps = new Dictionary<string, object?> { ["title"] = "Dashboard" };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("errors"));
    }

    [Fact]
    public void BuildPage_PartialReload_MultipleRequestedKeys()
    {
        var ctx = PartialReloadContext("/dashboard", "Dashboard", "title,user");
        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"]  = "John",
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
        var ctx = PartialReloadContext("/dashboard", "OtherComponent", "title");
        var componentProps = new Dictionary<string, object?>
        {
            ["title"] = "Dashboard",
            ["user"]  = "John",
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
            ["user"]  = "John",
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.True(page.Props.ContainsKey("user"));
    }

    // ── BuildPage — partial reload resolving deferred props (v3) ─────────────

    [Fact]
    public void BuildPage_PartialReload_DeferredProp_IsResolvedAndIncluded()
    {
        var ctx = PartialReloadContext("/dashboard", "Dashboard", "reports");
        var expected = new[] { "report1", "report2" };
        var componentProps = new Dictionary<string, object?>
        {
            ["user"]    = "John",
            ["reports"] = new InertiaDeferred(() => expected),
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("reports"));
        Assert.Same(expected, page.Props["reports"]);
        Assert.False(page.Props.ContainsKey("user"));
    }

    [Fact]
    public void BuildPage_PartialReload_DeferredProp_FactoryCalledExactlyOnce()
    {
        var ctx = PartialReloadContext("/dashboard", "Dashboard", "reports");
        int callCount = 0;
        var componentProps = new Dictionary<string, object?>
        {
            ["reports"] = new InertiaDeferred(() => { callCount++; return "data"; }),
        };

        InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void BuildPage_PartialReload_MergeProp_ResolvedWithMergeFlagSet()
    {
        var ctx = PartialReloadContext("/feed", "Feed", "posts");
        var newItems = new[] { "post3", "post4" };
        var componentProps = new Dictionary<string, object?>
        {
            ["posts"] = new InertiaMerge(newItems),
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Feed", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("posts"));
        Assert.Same(newItems, page.Props["posts"]);
        Assert.Contains("posts", page.MergeProps);
    }

    [Fact]
    public void BuildPage_PartialReload_UnrequestedDeferredProp_NotResolved()
    {
        var ctx = PartialReloadContext("/dashboard", "Dashboard", "user");
        int callCount = 0;
        var componentProps = new Dictionary<string, object?>
        {
            ["user"]    = "John",
            ["reports"] = new InertiaDeferred(() => { callCount++; return "data"; }),
        };

        InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.Equal(0, callCount);
    }

    // ── BuildPage — except header ─────────────────────────────────────────────

    [Fact]
    public void BuildPage_ExceptHeader_ExcludesSpecifiedProp()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Except"] = "heavyData";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"]     = "Dashboard",
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
            ["title"]     = "Dashboard",
            ["heavyData"] = "big",
            ["metrics"]   = "lots",
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.False(page.Props.ContainsKey("heavyData"));
        Assert.False(page.Props.ContainsKey("metrics"));
    }

    [Fact]
    public void BuildPage_ExceptHeader_DeferredPropExcepted_NotInDeferredPropsEither()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/dashboard";
        ctx.Request.Headers["X-Inertia-Partial-Except"] = "reports";

        var componentProps = new Dictionary<string, object?>
        {
            ["title"]   = "Dashboard",
            ["reports"] = new InertiaDeferred(() => "data"),
        };

        var page = InertiaPageRenderer.BuildPage(ctx.Request, "Dashboard", componentProps, new InertiaService(), new InertiaOptions());

        Assert.True(page.Props.ContainsKey("title"));
        Assert.False(page.Props.ContainsKey("reports"));
        // excepted deferred props are also removed from the deferred groups
        Assert.Empty(page.DeferredProps);
    }
}
