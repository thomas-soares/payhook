using System.Net;
using System.Text;
using FluentAssertions;
using Payhook.Api.Services;
using Payhook.Api.Tests.Infrastructure;
using Xunit;

namespace Payhook.Api.Tests.Controllers;

public sealed class PaymentWebhooksControllerTests
{
    [Fact]
    public async Task ReceiveShouldReturnUnauthorizedWhenSignatureIsMissing()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReceiveShouldReturnAcceptedWhenPayloadIsValid()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        var payloadJson = CreatePayloadJson();
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task ReceiveShouldReturnOkWhenTransactionWasAlreadyReceived()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        var payloadJson = CreatePayloadJson();
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);
        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static StringContent CreateJsonContent(string? payloadJson = null)
    {
        return new StringContent(
            payloadJson ?? CreatePayloadJson(),
            Encoding.UTF8,
            "application/json");
    }

    private static string CreatePayloadJson()
    {
        return """
            {"transaction_id":"txn_001","contract_id":"contract_001","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
            """;
    }
}
