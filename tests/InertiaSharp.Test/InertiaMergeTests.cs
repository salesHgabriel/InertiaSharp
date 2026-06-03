namespace InertiaSharp.Test;

public class InertiaMergeTests
{
    [Fact]
    public void Value_ReturnsWrappedValue()
    {
        var merge = new InertiaMerge("hello");
        Assert.Equal("hello", merge.Value);
    }

    [Fact]
    public void Value_Null_ReturnsNull()
    {
        var merge = new InertiaMerge(null);
        Assert.Null(merge.Value);
    }

    [Fact]
    public void Value_Integer_ReturnedCorrectly()
    {
        var merge = new InertiaMerge(99);
        Assert.Equal(99, merge.Value);
    }

    [Fact]
    public void Value_ComplexObject_SameInstance()
    {
        var obj = new { id = 1, name = "test" };
        var merge = new InertiaMerge(obj);
        Assert.Same(obj, merge.Value);
    }

    [Fact]
    public void Value_List_SameInstance()
    {
        var list = new List<int> { 1, 2, 3 };
        var merge = new InertiaMerge(list);
        Assert.Same(list, merge.Value);
    }
}
