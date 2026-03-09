using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using InertiaSharp.Extensions;

namespace InertiaSharp.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInertia_RegistersInertiaService_AsScoped()
    {
        var services = new ServiceCollection();
        services.AddInertia();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(InertiaService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddInertia_WithConfigure_SetsOptions()
    {
        var services = new ServiceCollection();
        services.AddInertia(opt =>
        {
            opt.RootView = "MyApp";
            opt.Version = "1.2.3";
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<InertiaOptions>>().Value;

        Assert.Equal("MyApp", options.RootView);
        Assert.Equal("1.2.3", options.Version);
    }

    [Fact]
    public void AddInertia_WithoutConfigure_UsesDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddInertia();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<InertiaOptions>>().Value;

        Assert.Equal("App", options.RootView);
        Assert.Null(options.Version);
        Assert.False(options.SsrEnabled);
    }

    [Fact]
    public void AddInertia_CalledTwice_DoesNotThrow()
    {
        var services = new ServiceCollection();

        var exception = Record.Exception(() =>
        {
            services.AddInertia();
            services.AddInertia(opt => opt.Version = "1.0");
        });

        Assert.Null(exception);
    }

    [Fact]
    public void AddInertia_InertiaService_ResolvedFromScope()
    {
        var services = new ServiceCollection();
        services.AddInertia();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetService<InertiaService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void AddInertia_InertiaService_DifferentInstancePerScope()
    {
        var services = new ServiceCollection();
        services.AddInertia();

        var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<InertiaService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<InertiaService>();

        Assert.NotSame(service1, service2);
    }
}
