using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Payhook.Api.DTOs;

public sealed class PaymentWebhookRequest
{
    [Required]
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("contract_id")]
    public string ContractId { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("payment_date")]
    public DateTimeOffset PaymentDate { get; init; }

    [Required]
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
