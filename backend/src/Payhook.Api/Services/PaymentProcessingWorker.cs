using Microsoft.Extensions.Options;
using Payhook.Api.Options;

namespace Payhook.Api.Services;

public sealed class PaymentProcessingWorker(
    IPaymentProcessingQueue queue,
    IServiceScopeFactory scopeFactory,
    IOptions<PaymentProcessingOptions> options,
    ILogger<PaymentProcessingWorker> logger) : BackgroundService
{
    private readonly PaymentProcessingOptions processingOptions = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(processingOptions.PendingScanInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hasQueuedEvent = await WaitForWorkAsync(timer, stoppingToken);

                if (!hasQueuedEvent)
                {
                    await ProcessPendingEventsAsync(stoppingToken);
                    continue;
                }

                while (queue.TryDequeue(out var rawEventId))
                {
                    await ProcessEventAsync(rawEventId, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled payment processing error.");
            }
        }
    }

    private async Task<bool> WaitForWorkAsync(
        PeriodicTimer timer,
        CancellationToken stoppingToken)
    {
        var queueTask = queue.WaitToReadAsync(stoppingToken).AsTask();
        var timerTask = timer.WaitForNextTickAsync(stoppingToken).AsTask();
        var completedTask = await Task.WhenAny(queueTask, timerTask);

        return completedTask == queueTask && await queueTask;
    }

    private async Task ProcessEventAsync(Guid rawEventId, CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<PaymentEventProcessor>();

        await processor.ProcessAsync(rawEventId, stoppingToken);
    }

    private async Task ProcessPendingEventsAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<PaymentEventProcessor>();

        await processor.ProcessPendingAsync(processingOptions.PendingBatchSize, stoppingToken);
    }
}
