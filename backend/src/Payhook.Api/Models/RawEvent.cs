namespace Payhook.Api.Models;

public sealed class RawEvent
{
    public Guid Id { get; set; }

    public string? TransactionId { get; set; }

    public string? ContractId { get; set; }

    public required string PayloadJson { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending;

    public string? ProcessingError { get; set; }

    public bool IsProcessable { get; set; } = true;
}
