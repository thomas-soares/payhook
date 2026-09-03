using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payhook.Api.Data;
using Payhook.Api.Models;
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
        var rawEvents = ReadRawEvents(factory);
        rawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == null
            && rawEvent.ContractId == null
            && rawEvent.ProcessingStatus == ProcessingStatus.Failed
            && rawEvent.ProcessingError == "Invalid signature."
            && !rawEvent.IsProcessable);
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

    [Fact]
    public async Task ReceiveShouldStoreRejectedRawEventWhenJsonIsInvalid()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        const string payloadJson = "{";
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var rawEvents = ReadRawEvents(factory);
        rawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == null
            && rawEvent.ContractId == null
            && rawEvent.PayloadJson == payloadJson
            && rawEvent.ProcessingStatus == ProcessingStatus.Failed
            && rawEvent.ProcessingError == "Invalid JSON payload."
            && !rawEvent.IsProcessable);
    }

    [Fact]
    public async Task ReceiveShouldStoreRejectedRawEventWhenPayloadValidationFails()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        const string payloadJson = """
            {"transaction_id":"txn_001","contract_id":"contract_001","amount":0,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
            """;
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var rawEvents = ReadRawEvents(factory);
        rawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == "txn_001"
            && rawEvent.ContractId == "contract_001"
            && rawEvent.PayloadJson == payloadJson
            && rawEvent.ProcessingStatus == ProcessingStatus.Failed
            && rawEvent.ProcessingError!.Contains("field Amount must be between")
            && !rawEvent.IsProcessable);
    }

    [Fact]
    public async Task ReceiveShouldStoreRejectedRawEventWhenRequiredFieldsAreMissing()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        const string payloadJson = """
            {"contract_id":"contract_001","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
            """;
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var rawEvents = ReadRawEvents(factory);
        rawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == null
            && rawEvent.ContractId == "contract_001"
            && rawEvent.PayloadJson == payloadJson
            && rawEvent.ProcessingStatus == ProcessingStatus.Failed
            && rawEvent.ProcessingError!.Contains("TransactionId")
            && rawEvent.ProcessingError!.Contains("required")
            && !rawEvent.IsProcessable);
    }

    [Fact]
    public async Task ReceiveShouldStoreRejectedRawEventWhenPaymentDateIsMissing()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        const string payloadJson = """
            {"transaction_id":"txn_001","contract_id":"contract_001","amount":10.50,"status":"Paid"}
            """;
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var rawEvents = ReadRawEvents(factory);
        rawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == "txn_001"
            && rawEvent.ContractId == "contract_001"
            && rawEvent.PayloadJson == payloadJson
            && rawEvent.ProcessingStatus == ProcessingStatus.Failed
            && rawEvent.ProcessingError!.Contains("PaymentDate")
            && rawEvent.ProcessingError!.Contains("required")
            && !rawEvent.IsProcessable);
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

    private static List<RawEvent> ReadRawEvents(PayhookApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return dbContext.RawEvents.ToList();
    }
}
