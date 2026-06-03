using Microsoft.AspNetCore.Mvc;
using InertiaSharp.Extensions;

namespace InertiaSharp.Test;

public class ControllerExtensionsTests
{
    private sealed class FakeController : ControllerBase { }

    // ── Basic Inertia() returns ───────────────────────────────────────────────

    [Fact]
    public void Inertia_WithNullProps_ReturnsInertiaResult()
    {
        var controller = new FakeController();
        var result = controller.Inertia("Dashboard");
        Assert.IsType<InertiaResult>(result);
    }

    [Fact]
    public void Inertia_WithAnonymousObject_ReturnsInertiaResult()
    {
        var controller = new FakeController();
        var result = controller.Inertia("Dashboard", new { name = "John" });
        Assert.IsType<InertiaResult>(result);
    }

    [Fact]
    public void Inertia_WithDictionary_ReturnsInertiaResult()
    {
        var controller = new FakeController();
        var props = new Dictionary<string, object?> { ["name"] = "John" };
        var result = controller.Inertia("Dashboard", props);
        Assert.IsType<InertiaResult>(result);
    }

    [Fact]
    public void Inertia_WithDictionaryOverload_AcceptsExplicitDictionary()
    {
        var controller = new FakeController();
        IDictionary<string, object?> props = new Dictionary<string, object?> { ["key"] = "value" };
        var result = controller.Inertia("Home", props);
        Assert.IsType<InertiaResult>(result);
    }

    // ── Fluent history modifiers (v3) ─────────────────────────────────────────

    [Fact]
    public void InertiaResult_WithEncryptedHistory_ReturnsSameInstance()
    {
        var controller = new FakeController();
        var result = controller.Inertia("SecurePage");
        var chained = result.WithEncryptedHistory();
        Assert.Same(result, chained);
    }

    [Fact]
    public void InertiaResult_WithClearHistory_ReturnsSameInstance()
    {
        var controller = new FakeController();
        var result = controller.Inertia("Home");
        var chained = result.WithClearHistory();
        Assert.Same(result, chained);
    }

    [Fact]
    public void InertiaResult_FluentChaining_BothModifiers()
    {
        var controller = new FakeController();
        var result = controller.Inertia("Page")
            .WithEncryptedHistory()
            .WithClearHistory();
        Assert.IsType<InertiaResult>(result);
    }

    [Fact]
    public void InertiaResult_WithEncryptedHistory_IsFluentFromDictionaryOverload()
    {
        var controller = new FakeController();
        var props = new Dictionary<string, object?> { ["x"] = 1 };
        var result = controller.Inertia("Page", props).WithEncryptedHistory();
        Assert.IsType<InertiaResult>(result);
    }
}
