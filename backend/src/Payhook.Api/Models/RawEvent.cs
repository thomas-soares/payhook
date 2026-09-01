namespace Payhook.Api.Models;

public sealed class RawEvent
{
    public Guid Id { get; set; }

    public required string TransactionId { get; set; }

    public required string PayloadJson { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending;

    public string? ProcessingError { get; set; }
}
