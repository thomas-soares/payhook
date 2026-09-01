using Payhook.Api.Models;

namespace Payhook.Api.DTOs;

public sealed class PaymentSummaryResponse
{
    public Guid Id { get; init; }

    public required string TransactionId { get; init; }

    public required string ContractId { get; init; }

    public ProcessingStatus ProcessingStatus { get; init; }

    public DateTimeOffset ReceivedAt { get; init; }

    public string? PaymentStatus { get; init; }

    public decimal? Amount { get; init; }

    public DateTimeOffset? PaymentDate { get; init; }

    public string? ProcessingError { get; init; }
}
