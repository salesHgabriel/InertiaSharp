using Microsoft.AspNetCore.Http;
using InertiaSharp.Extensions;

namespace InertiaSharp.Test;

public class InertiaResultExtensionsTests
{
    private static IResultExtensions ResultExtensions => new ResultExtensionsFake();

    private sealed class ResultExtensionsFake : IResultExtensions { }

    [Fact]
    public void Inertia_WithNullProps_ReturnsInertiaHttpResult()
    {
        var result = ResultExtensions.Inertia("Dashboard");
        Assert.IsType<InertiaHttpResult>(result);
    }

    [Fact]
    public void Inertia_WithAnonymousObject_ReturnsInertiaHttpResult()
    {
        var result = ResultExtensions.Inertia("Dashboard", new { name = "John" });
        Assert.IsType<InertiaHttpResult>(result);
    }

    [Fact]
    public void Inertia_WithDictionary_ReturnsInertiaHttpResult()
    {
        var props = new Dictionary<string, object?> { ["name"] = "John" };
        var result = ResultExtensions.Inertia("Dashboard", props);
        Assert.IsType<InertiaHttpResult>(result);
    }

    [Fact]
    public void InertiaEncrypted_ReturnsInertiaHttpResult()
    {
        var result = ResultExtensions.InertiaEncrypted("SecurePage");
        Assert.IsType<InertiaHttpResult>(result);
    }

    [Fact]
    public void InertiaEncrypted_WithProps_ReturnsInertiaHttpResult()
    {
        var result = ResultExtensions.InertiaEncrypted("SecurePage", new { secret = "value" });
        Assert.IsType<InertiaHttpResult>(result);
    }
}
