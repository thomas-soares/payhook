using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Payhook.Api.Options;

namespace Payhook.Api.Services;

public sealed class WebhookSecurityService(IOptions<WebhookSecurityOptions> options)
{
    private const string SignaturePrefix = "sha256=";
    private readonly string signatureSecret = options.Value.SignatureSecret;

    public bool IsValid(string? providedSignature, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(providedSignature) || string.IsNullOrEmpty(payloadJson))
        {
            return false;
        }

        var expectedSignature = ComputeSignature(payloadJson, signatureSecret);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
        var providedBytes = Encoding.UTF8.GetBytes(providedSignature);

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    public static string ComputeSignature(string payloadJson, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
        var hashBytes = HMACSHA256.HashData(keyBytes, payloadBytes);
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return $"{SignaturePrefix}{hash}";
    }
}
