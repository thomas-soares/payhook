using FluentAssertions;
using Payhook.Api.Options;
using Payhook.Api.Services;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Payhook.Api.Tests.Services;

public sealed class PaymentProcessingQueueTests
{
    [Fact]
    public async Task EnqueueAsyncShouldWaitWhenQueueIsFull()
    {
        var queue = new PaymentProcessingQueue(OptionsFactory.Create(new PaymentProcessingOptions
        {
            QueueCapacity = 1
        }));
        var firstRawEventId = Guid.NewGuid();
        var secondRawEventId = Guid.NewGuid();

        await queue.EnqueueAsync(firstRawEventId, TestContext.Current.CancellationToken);
        var pendingEnqueue = queue.EnqueueAsync(
            secondRawEventId,
            TestContext.Current.CancellationToken).AsTask();

        pendingEnqueue.IsCompleted.Should().BeFalse();

        queue.TryDequeue(out var dequeuedRawEventId).Should().BeTrue();
        dequeuedRawEventId.Should().Be(firstRawEventId);

        await pendingEnqueue.WaitAsync(TestContext.Current.CancellationToken);
        queue.TryDequeue(out dequeuedRawEventId).Should().BeTrue();
        dequeuedRawEventId.Should().Be(secondRawEventId);
    }
}
