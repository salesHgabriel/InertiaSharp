using Microsoft.AspNetCore.Http;
using InertiaSharp.Extensions;

namespace InertiaSharp.Test;

public class InertiaResultExtensionsTests
{
    private static IResultExtensions ResultExtensions => new ResultExtensionsFake();

    private sealed class ResultExtensionsFake : IResultExtensions { }

    // ── Factory methods ───────────────────────────────────────────────────────

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

    // ── Fluent history modifiers (v3) ─────────────────────────────────────────

    [Fact]
    public void InertiaHttpResult_WithEncryptedHistory_ReturnsSameInstance()
    {
        var result = (InertiaHttpResult)ResultExtensions.Inertia("SecurePage");
        var chained = result.WithEncryptedHistory();
        Assert.Same(result, chained);
    }

    [Fact]
    public void InertiaHttpResult_WithClearHistory_ReturnsSameInstance()
    {
        var result = (InertiaHttpResult)ResultExtensions.Inertia("Home");
        var chained = result.WithClearHistory();
        Assert.Same(result, chained);
    }

    [Fact]
    public void InertiaHttpResult_FluentChaining_BothModifiers()
    {
        var result = ((InertiaHttpResult)ResultExtensions.Inertia("Page"))
            .WithEncryptedHistory()
            .WithClearHistory();
        Assert.IsType<InertiaHttpResult>(result);
    }

    [Fact]
    public void InertiaHttpResult_WithEncryptedHistory_IsFluentFromDictionaryOverload()
    {
        var props = new Dictionary<string, object?> { ["x"] = 1 };
        var result = ((InertiaHttpResult)ResultExtensions.Inertia("Page", props)).WithEncryptedHistory();
        Assert.IsType<InertiaHttpResult>(result);
    }
}
