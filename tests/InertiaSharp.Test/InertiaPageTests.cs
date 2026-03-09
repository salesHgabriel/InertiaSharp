using System.Text.Json;

namespace InertiaSharp.Test;

public class InertiaPageTests
{
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
    public void Serializes_WithCamelCasePropertyNames()
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
    }

    [Fact]
    public void Serializes_ComponentValue()
    {
        var page = new InertiaPage { Component = "Auth/Login", Url = "/login" };
        var json = JsonSerializer.Serialize(page);
        Assert.Contains("\"Auth/Login\"", json);
    }

    [Fact]
    public void Deserializes_FromCamelCaseJson()
    {
        var json = """
            {
                "component": "Auth/Login",
                "props": {},
                "url": "/login",
                "version": "abc123",
                "encryptHistory": true,
                "clearHistory": true
            }
            """;

        var page = JsonSerializer.Deserialize<InertiaPage>(json)!;

        Assert.Equal("Auth/Login", page.Component);
        Assert.Equal("/login", page.Url);
        Assert.Equal("abc123", page.Version);
        Assert.True(page.EncryptHistory);
        Assert.True(page.ClearHistory);
    }

    [Fact]
    public void CanSet_AllProperties()
    {
        var props = new Dictionary<string, object?> { ["key"] = "value" };
        var page = new InertiaPage
        {
            Component = "Home",
            Props = props,
            Url = "/home",
            Version = "v1",
            EncryptHistory = true,
            ClearHistory = true,
        };

        Assert.Equal("Home", page.Component);
        Assert.Same(props, page.Props);
        Assert.Equal("/home", page.Url);
        Assert.Equal("v1", page.Version);
        Assert.True(page.EncryptHistory);
        Assert.True(page.ClearHistory);
    }
}
