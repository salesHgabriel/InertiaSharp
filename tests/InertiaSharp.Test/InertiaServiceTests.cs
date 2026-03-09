namespace InertiaSharp.Test;

public class InertiaServiceTests
{
    [Fact]
    public void ResolveSharedProps_Empty_WhenNothingShared()
    {
        var service = new InertiaService();
        var props = service.ResolveSharedProps();
        Assert.Empty(props);
    }

    [Fact]
    public void Share_StaticValue_ResolvedCorrectly()
    {
        var service = new InertiaService();
        service.Share("user", "John");

        var props = service.ResolveSharedProps();

        Assert.True(props.ContainsKey("user"));
        Assert.Equal("John", props["user"]);
    }

    [Fact]
    public void Share_NullValue_IncludedInResolution()
    {
        var service = new InertiaService();
        service.Share("nullKey", (object?)null);

        var props = service.ResolveSharedProps();

        Assert.True(props.ContainsKey("nullKey"));
        Assert.Null(props["nullKey"]);
    }

    [Fact]
    public void Share_Factory_EvaluatedOnResolution()
    {
        var service = new InertiaService();
        int callCount = 0;
        service.Share("count", () => ++callCount);

        service.ResolveSharedProps();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Share_Factory_CalledEachResolution()
    {
        var service = new InertiaService();
        int callCount = 0;
        service.Share("count", () => ++callCount);

        service.ResolveSharedProps();
        service.ResolveSharedProps();

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Share_Dictionary_SharesAllValues()
    {
        var service = new InertiaService();
        service.Share(new Dictionary<string, object?>
        {
            ["a"] = 1,
            ["b"] = "hello",
        });

        var props = service.ResolveSharedProps();

        Assert.Equal(1, props["a"]);
        Assert.Equal("hello", props["b"]);
    }

    [Fact]
    public void Share_OverwritesPreviousValueForSameKey()
    {
        var service = new InertiaService();
        service.Share("key", "first");
        service.Share("key", "second");

        var props = service.ResolveSharedProps();

        Assert.Equal("second", props["key"]);
    }

    [Fact]
    public void Flash_ValueAppearsInResolution()
    {
        var service = new InertiaService();
        service.Flash("message", "Success!");

        var props = service.ResolveSharedProps();

        Assert.Equal("Success!", props["message"]);
    }

    [Fact]
    public void Flash_OverridesSharedPropWithSameKey()
    {
        var service = new InertiaService();
        service.Share("message", "Original");
        service.Flash("message", "Flash");

        var props = service.ResolveSharedProps();

        Assert.Equal("Flash", props["message"]);
    }

    [Fact]
    public void Share_StaticValue_IsFluentChainable()
    {
        var service = new InertiaService();
        var result = service.Share("a", 1).Share("b", 2);
        Assert.Same(service, result);
    }

    [Fact]
    public void Share_Factory_IsFluentChainable()
    {
        var service = new InertiaService();
        var result = service.Share("a", () => 1);
        Assert.Same(service, result);
    }

    [Fact]
    public void Share_Dictionary_IsFluentChainable()
    {
        var service = new InertiaService();
        var result = service.Share(new Dictionary<string, object?> { ["x"] = 1 });
        Assert.Same(service, result);
    }

    [Fact]
    public void Flash_IsFluentChainable()
    {
        var service = new InertiaService();
        var result = service.Flash("msg", "hello");
        Assert.Same(service, result);
    }

    [Fact]
    public void ResolveSharedProps_ContainsAllSharedAndFlash()
    {
        var service = new InertiaService();
        service.Share("shared1", "a");
        service.Share("shared2", "b");
        service.Flash("flash1", "c");

        var props = service.ResolveSharedProps();

        Assert.Equal(3, props.Count);
        Assert.Equal("a", props["shared1"]);
        Assert.Equal("b", props["shared2"]);
        Assert.Equal("c", props["flash1"]);
    }
}
