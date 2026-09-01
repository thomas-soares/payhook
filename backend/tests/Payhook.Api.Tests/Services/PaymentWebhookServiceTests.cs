using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payhook.Api.Data;
using Payhook.Api.DTOs;
using Payhook.Api.Models;
using Payhook.Api.Services;
using Xunit;

namespace Payhook.Api.Tests.Services;

public sealed class PaymentWebhookServiceTests
{
    [Fact]
    public async Task ReceiveAsyncShouldStoreRawEvent()
    {
        await using var context = CreateContext();
        var service = new PaymentWebhookService(context);
        var request = CreateRequest();
        const string payloadJson = """
            {"transaction_id":"txn_001","contract_id":"contract_001","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
            """;

        var result = await service.ReceiveAsync(request, payloadJson, CancellationToken.None);

        result.Should().Be(PaymentWebhookResult.Accepted);
        context.RawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == request.TransactionId
            && rawEvent.PayloadJson == payloadJson
            && rawEvent.ProcessingStatus == ProcessingStatus.Pending);
    }

    [Fact]
    public async Task ReceiveAsyncShouldNotStoreDuplicateTransaction()
    {
        await using var context = CreateContext();
        var service = new PaymentWebhookService(context);
        var request = CreateRequest();

        await service.ReceiveAsync(request, "{}", CancellationToken.None);
        var result = await service.ReceiveAsync(request, "{}", CancellationToken.None);

        result.Should().Be(PaymentWebhookResult.Duplicate);
        context.RawEvents.Should().ContainSingle(rawEvent =>
            rawEvent.TransactionId == request.TransactionId);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PaymentWebhookRequest CreateRequest()
    {
        return new PaymentWebhookRequest
        {
            TransactionId = "txn_001",
            ContractId = "contract_001",
            Amount = 10.50m,
            PaymentDate = DateTimeOffset.Parse("2026-09-01T00:00:00Z"),
            Status = "Paid"
        };
    }
}
