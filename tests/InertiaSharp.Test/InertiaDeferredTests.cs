namespace InertiaSharp.Test;

public class InertiaDeferredTests
{
    [Fact]
    public void Resolve_CallsFactory_AndReturnsValue()
    {
        var deferred = new InertiaDeferred(() => "hello");
        Assert.Equal("hello", deferred.Resolve());
    }

    [Fact]
    public void Resolve_NullFactory_ReturnsNull()
    {
        var deferred = new InertiaDeferred(() => null);
        Assert.Null(deferred.Resolve());
    }

    [Fact]
    public void Resolve_ComplexObject_ReturnedUnchanged()
    {
        var expected = new { id = 1, name = "test" };
        var deferred = new InertiaDeferred(() => expected);
        Assert.Same(expected, deferred.Resolve());
    }

    [Fact]
    public void Resolve_CalledMultipleTimes_InvokesFactoryEachTime()
    {
        int count = 0;
        var deferred = new InertiaDeferred(() => ++count);

        deferred.Resolve();
        deferred.Resolve();

        Assert.Equal(2, count);
    }

    [Fact]
    public void Resolve_IntValue_ReturnedCorrectly()
    {
        var deferred = new InertiaDeferred(() => (object?)42);
        Assert.Equal(42, deferred.Resolve());
    }

    [Fact]
    public void Resolve_List_ReturnedCorrectly()
    {
        var list = new List<string> { "a", "b" };
        var deferred = new InertiaDeferred(() => list);
        Assert.Same(list, deferred.Resolve());
    }
}
