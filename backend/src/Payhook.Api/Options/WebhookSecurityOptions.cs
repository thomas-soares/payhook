using System.ComponentModel.DataAnnotations;

namespace Payhook.Api.Options;

public sealed class WebhookSecurityOptions
{
    public const string SectionName = "WebhookSecurity";

    [Required]
    public required string SignatureSecret { get; init; }
}
