namespace Payhook.Api.Models;

public sealed class ContractStatus
{
    public required string ContractId { get; set; }

    public required string Status { get; set; }

    public decimal Amount { get; set; }

    public DateTimeOffset PaymentDate { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
