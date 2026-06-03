using System.Text.Json;

namespace InertiaSharp.Test;

public class InertiaPageTests
{
    // ── Default values ────────────────────────────────────────────────────────

    [Fact]
    public void DefaultProps_IsEmptyDictionary()
    {
        var page = new InertiaPage();
        Assert.Empty(page.Props);
    }

    [Fact]
    public void DefaultEncryptHistory_IsFalse()
    {
        var page = new InertiaPage();
        Assert.False(page.EncryptHistory);
    }

    [Fact]
    public void DefaultClearHistory_IsFalse()
    {
        var page = new InertiaPage();
        Assert.False(page.ClearHistory);
    }

    [Fact]
    public void DefaultVersion_IsNull()
    {
        var page = new InertiaPage();
        Assert.Null(page.Version);
    }

    [Fact]
    public void DefaultDeferredProps_IsEmptyList()
    {
        var page = new InertiaPage();
        Assert.Empty(page.DeferredProps);
    }

    [Fact]
    public void DefaultMergeProps_IsEmptyList()
    {
        var page = new InertiaPage();
        Assert.Empty(page.MergeProps);
    }

    // ── Setting all properties ────────────────────────────────────────────────

    [Fact]
    public void CanSet_AllProperties()
    {
        var props = new Dictionary<string, object?> { ["key"] = "value" };
        var deferred = new List<IList<string>> { new List<string> { "reports" } };
        var merge = new List<string> { "feed" };

        var page = new InertiaPage
        {
            Component      = "Home",
            Props          = props,
            Url            = "/home",
            Version        = "v1",
            EncryptHistory = true,
            ClearHistory   = true,
            DeferredProps  = deferred,
            MergeProps     = merge,
        };

        Assert.Equal("Home", page.Component);
        Assert.Same(props, page.Props);
        Assert.Equal("/home", page.Url);
        Assert.Equal("v1", page.Version);
        Assert.True(page.EncryptHistory);
        Assert.True(page.ClearHistory);
        Assert.Same(deferred, page.DeferredProps);
        Assert.Same(merge, page.MergeProps);
    }

    // ── Serialization ─────────────────────────────────────────────────────────

    [Fact]
    public void Serializes_WithExpectedJsonPropertyNames()
    {
        var page = new InertiaPage
        {
            Component = "Dashboard",
            Props = new Dictionary<string, object?> { ["name"] = "John" },
            Url = "/dashboard",
            Version = "1.0",
            EncryptHistory = true,
            ClearHistory = false,
        };

        var json = JsonSerializer.Serialize(page);

        Assert.Contains("\"component\"", json);
        Assert.Contains("\"props\"", json);
        Assert.Contains("\"url\"", json);
        Assert.Contains("\"version\"", json);
        Assert.Contains("\"encryptHistory\"", json);
        Assert.Contains("\"clearHistory\"", json);
        Assert.Contains("\"deferredProps\"", json);
        Assert.Contains("\"mergeProps\"", json);
    }

    [Fact]
    public void Serializes_ComponentValue()
    {
        var page = new InertiaPage { Component = "Auth/Login", Url = "/login" };
        var json = JsonSerializer.Serialize(page);
        Assert.Contains("\"Auth/Login\"", json);
    }

    [Fact]
    public void Serializes_DeferredProps_AsNestedArray()
    {
        var page = new InertiaPage
        {
            Component = "Dashboard",
            Url = "/dashboard",
            DeferredProps = new List<IList<string>>
            {
                new List<string> { "reports" },
                new List<string> { "stats", "metrics" },
            },
        };

        var json = JsonSerializer.Serialize(page);

        Assert.Contains("\"deferredProps\"", json);
        Assert.Contains("\"reports\"", json);
        Assert.Contains("\"stats\"", json);
        Assert.Contains("\"metrics\"", json);
    }

    [Fact]
    public void Serializes_MergeProps_AsStringArray()
    {
        var page = new InertiaPage
        {
            Component  = "Feed",
            Url        = "/feed",
            MergeProps = new List<string> { "posts", "comments" },
        };

        var json = JsonSerializer.Serialize(page);

        Assert.Contains("\"mergeProps\"", json);
        Assert.Contains("\"posts\"", json);
        Assert.Contains("\"comments\"", json);
    }

    [Fact]
    public void Serializes_EmptyDeferredAndMerge_AsEmptyArrays()
    {
        var page = new InertiaPage { Component = "Home", Url = "/" };
        var json = JsonSerializer.Serialize(page);

        Assert.Contains("\"deferredProps\":[]", json);
        Assert.Contains("\"mergeProps\":[]", json);
    }

    // ── Deserialization ───────────────────────────────────────────────────────

    [Fact]
    public void Deserializes_FromCamelCaseJson_BasicFields()
    {
        var json = """
            {
                "component": "Auth/Login",
                "props": {},
                "url": "/login",
                "version": "abc123",
                "encryptHistory": true,
                "clearHistory": true,
                "deferredProps": [],
                "mergeProps": []
            }
            """;

        var page = JsonSerializer.Deserialize<InertiaPage>(json)!;

        Assert.Equal("Auth/Login", page.Component);
        Assert.Equal("/login", page.Url);
        Assert.Equal("abc123", page.Version);
        Assert.True(page.EncryptHistory);
        Assert.True(page.ClearHistory);
        Assert.Empty(page.DeferredProps);
        Assert.Empty(page.MergeProps);
    }

    [Fact]
    public void Deserializes_DeferredProps_AsNestedList()
    {
        var json = """
            {
                "component": "Dashboard",
                "props": {},
                "url": "/dashboard",
                "version": null,
                "encryptHistory": false,
                "clearHistory": false,
                "deferredProps": [["reports"], ["stats", "metrics"]],
                "mergeProps": []
            }
            """;

        var page = JsonSerializer.Deserialize<InertiaPage>(json)!;

        Assert.Equal(2, page.DeferredProps.Count);
        Assert.Single(page.DeferredProps[0]);
        Assert.Equal("reports", page.DeferredProps[0][0]);
        Assert.Equal(2, page.DeferredProps[1].Count);
        Assert.Equal("stats", page.DeferredProps[1][0]);
        Assert.Equal("metrics", page.DeferredProps[1][1]);
    }

    [Fact]
    public void Deserializes_MergeProps_AsStringList()
    {
        var json = """
            {
                "component": "Feed",
                "props": {},
                "url": "/feed",
                "version": null,
                "encryptHistory": false,
                "clearHistory": false,
                "deferredProps": [],
                "mergeProps": ["posts", "comments"]
            }
            """;

        var page = JsonSerializer.Deserialize<InertiaPage>(json)!;

        Assert.Equal(2, page.MergeProps.Count);
        Assert.Contains("posts", page.MergeProps);
        Assert.Contains("comments", page.MergeProps);
    }
}
