using MassTransit;
using Microsoft.Extensions.Logging;
using Shared;

namespace Consumer;

public class TestMessageConsumer : IConsumer<TestMessage>
{
    private readonly ILogger<TestMessageConsumer> _logger;

    public TestMessageConsumer(ILogger<TestMessageConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<TestMessage> context)
    {
        _logger.LogInformation("Test message consumed");

        return Task.CompletedTask;
    }
}