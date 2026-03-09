using Microsoft.AspNetCore.Mvc;
using InertiaSharp.Extensions;

namespace InertiaSharp.Test;

public class ControllerExtensionsTests
{
    private sealed class FakeController : ControllerBase { }

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
}
