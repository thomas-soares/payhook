using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Payhook.Api.Options;

namespace Payhook.Api.Services;

public sealed class PaymentProcessingQueue(IOptions<PaymentProcessingOptions> options) : IPaymentProcessingQueue
{
    private readonly Channel<Guid> queue = Channel.CreateBounded<Guid>(new BoundedChannelOptions(options.Value.QueueCapacity)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });

    public ValueTask EnqueueAsync(Guid rawEventId, CancellationToken cancellationToken)
    {
        return queue.Writer.WriteAsync(rawEventId, cancellationToken);
    }

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
    {
        return queue.Reader.WaitToReadAsync(cancellationToken);
    }

    public bool TryDequeue(out Guid rawEventId)
    {
        return queue.Reader.TryRead(out rawEventId);
    }
}
