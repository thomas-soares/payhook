using System.Diagnostics;
using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payhook.Api.Data;
using Payhook.Api.Models;
using Payhook.Api.Services;
using Payhook.Api.Tests.Infrastructure;
using Xunit;

namespace Payhook.Api.Tests.Integration;

public sealed class PaymentWebhookFlowTests
{
    [Fact]
    public async Task PaymentWebhookShouldBeLoggedProcessedAndReflectedInContractStatus()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        var payloadJson = CreatePayloadJson("txn_flow_001", "contract_flow_001");
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var response = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        await WaitUntilAsync(
            factory,
            context =>
                context.RawEvents.Any(rawEvent =>
                    rawEvent.TransactionId == "txn_flow_001"
                    && rawEvent.ProcessingStatus == ProcessingStatus.Processed)
                && context.ContractStatuses.Any(contractStatus =>
                    contractStatus.ContractId == "contract_flow_001"
                    && contractStatus.Status == "Paid"
                    && contractStatus.Amount == 10.50m),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DuplicatePaymentWebhookShouldNotCreateAnotherRawEventOrReprocessContractStatus()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();
        var payloadJson = CreatePayloadJson("txn_flow_duplicate", "contract_flow_duplicate");
        client.DefaultRequestHeaders.Add(
            "X-Signature",
            WebhookSecurityService.ComputeSignature(payloadJson, "test-secret"));

        var firstResponse = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);
        await WaitUntilAsync(
            factory,
            context => context.RawEvents.Any(rawEvent =>
                rawEvent.TransactionId == "txn_flow_duplicate"
                && rawEvent.ProcessingStatus == ProcessingStatus.Processed),
            TestContext.Current.CancellationToken);

        var secondResponse = await client.PostAsync(
            "/webhooks/payment",
            CreateJsonContent(payloadJson),
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.RawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == "txn_flow_duplicate");
        dbContext.ContractStatuses.Should().ContainSingle(contractStatus =>
            contractStatus.ContractId == "contract_flow_duplicate");
    }

    [Fact]
    public async Task RejectedWebhookShouldBeVisibleThroughPaymentDetailApi()
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

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rawEvent = dbContext.RawEvents.Single();

        var detailResponse = await client.GetAsync(
            $"/payments/{rawEvent.Id}",
            TestContext.Current.CancellationToken);

        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailJson = await detailResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        detailJson.Should().Contain("Invalid JSON payload.");
        detailJson.Should().Contain("\"processingStatus\":\"Failed\"");
    }

    private static async Task WaitUntilAsync(
        PayhookApiFactory factory,
        Func<ApplicationDbContext, bool> condition,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (condition(dbContext))
            {
                return;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException("The expected payment webhook state was not reached.");
    }

    private static StringContent CreateJsonContent(string payloadJson)
    {
        return new StringContent(payloadJson, Encoding.UTF8, "application/json");
    }

    private static string CreatePayloadJson(string transactionId, string contractId)
    {
        return $$"""
            {"transaction_id":"{{transactionId}}","contract_id":"{{contractId}}","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
            """;
    }
}
