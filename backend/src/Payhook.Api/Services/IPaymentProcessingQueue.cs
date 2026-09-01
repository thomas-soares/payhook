namespace Payhook.Api.Services;

public interface IPaymentProcessingQueue
{
    ValueTask EnqueueAsync(Guid rawEventId, CancellationToken cancellationToken);

    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);

    bool TryDequeue(out Guid rawEventId);
}
