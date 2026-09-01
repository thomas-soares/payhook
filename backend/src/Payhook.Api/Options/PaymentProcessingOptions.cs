using System.ComponentModel.DataAnnotations;

namespace Payhook.Api.Options;

public sealed class PaymentProcessingOptions
{
    public const string SectionName = "PaymentProcessing";

    [Required]
    public TimeSpan ProcessingDelay { get; init; } = TimeSpan.FromSeconds(2);

    [Range(1, 100_000)]
    public int QueueCapacity { get; init; } = 1000;

    [Required]
    public TimeSpan PendingScanInterval { get; init; } = TimeSpan.FromSeconds(10);

    [Range(1, 1000)]
    public int PendingBatchSize { get; init; } = 50;
}
