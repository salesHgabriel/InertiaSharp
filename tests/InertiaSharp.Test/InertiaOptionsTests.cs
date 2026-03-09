namespace InertiaSharp.Test;

public class InertiaOptionsTests
{
    [Fact]
    public void DefaultRootView_IsApp()
    {
        var options = new InertiaOptions();
        Assert.Equal("App", options.RootView);
    }

    [Fact]
    public void DefaultVersion_IsNull()
    {
        var options = new InertiaOptions();
        Assert.Null(options.Version);
    }

    [Fact]
    public void DefaultSsrEnabled_IsFalse()
    {
        var options = new InertiaOptions();
        Assert.False(options.SsrEnabled);
    }

    [Fact]
    public void DefaultSsrUrl_IsNull()
    {
        var options = new InertiaOptions();
        Assert.Null(options.SsrUrl);
    }

    [Fact]
    public void CanSet_CustomRootView()
    {
        var options = new InertiaOptions { RootView = "MyApp" };
        Assert.Equal("MyApp", options.RootView);
    }

    [Fact]
    public void CanSet_Version()
    {
        var options = new InertiaOptions { Version = "2.0.0" };
        Assert.Equal("2.0.0", options.Version);
    }

    [Fact]
    public void CanSet_SsrEnabled()
    {
        var options = new InertiaOptions { SsrEnabled = true };
        Assert.True(options.SsrEnabled);
    }

    [Fact]
    public void CanSet_SsrUrl()
    {
        var options = new InertiaOptions { SsrUrl = "http://localhost:13714" };
        Assert.Equal("http://localhost:13714", options.SsrUrl);
    }
}
