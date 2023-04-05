using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Producer;

public class BusObserver : IBusObserver
{
    private readonly IServiceProvider _serviceProvider;

    public BusObserver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void PostCreate(IBus bus)
    {
    }

    public void CreateFaulted(Exception exception)
    {
    }

    public Task PreStart(IBus bus) => Task.CompletedTask;

    // Send a test message when the bus is ready
    public async Task PostStart(IBus bus, Task<BusReady> busReady)
    {
        await busReady;
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        using var serviceScope = _serviceProvider.CreateScope();
        
        var testMessage = new TestMessage();
        var publishEndpoint = serviceScope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await publishEndpoint.Publish(testMessage);

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        await dbContext.SaveChangesAsync();
    }

    public Task StartFaulted(IBus bus, Exception exception) => Task.CompletedTask;

    public Task PreStop(IBus bus) => Task.CompletedTask;

    public Task PostStop(IBus bus) => Task.CompletedTask;

    public Task StopFaulted(IBus bus, Exception exception) => Task.CompletedTask;
}