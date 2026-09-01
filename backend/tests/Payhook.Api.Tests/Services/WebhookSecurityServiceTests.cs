using FluentAssertions;
using Payhook.Api.Options;
using Payhook.Api.Services;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Payhook.Api.Tests.Services;

public sealed class WebhookSecurityServiceTests
{
    [Fact]
    public void IsValidShouldReturnTrueWhenSignatureMatchesPayload()
    {
        var service = CreateService();
        const string payloadJson = "{\"transaction_id\":\"txn_001\"}";
        var signature = WebhookSecurityService.ComputeSignature(payloadJson, "test-secret");

        var result = service.IsValid(signature, payloadJson);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sha256=invalid")]
    public void IsValidShouldReturnFalseWhenSignatureDoesNotMatchPayload(string? providedSignature)
    {
        var service = CreateService();

        var result = service.IsValid(providedSignature, "{\"transaction_id\":\"txn_001\"}");

        result.Should().BeFalse();
    }

    private static WebhookSecurityService CreateService()
    {
        var options = OptionsFactory.Create(new WebhookSecurityOptions
        {
            SignatureSecret = "test-secret"
        });

        return new WebhookSecurityService(options);
    }
}
