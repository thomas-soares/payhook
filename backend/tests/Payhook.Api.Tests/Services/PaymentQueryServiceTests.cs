using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payhook.Api.Data;
using Payhook.Api.Models;
using Payhook.Api.Services;
using Xunit;

namespace Payhook.Api.Tests.Services;

public sealed class PaymentQueryServiceTests
{
    [Fact]
    public async Task GetPaymentsAsyncShouldReturnPagedResultsOrderedByReceivedAt()
    {
        await using var context = CreateContext();
        var olderRawEvent = CreateRawEvent("txn_001", "contract_001", DateTimeOffset.UtcNow.AddMinutes(-2));
        var newerRawEvent = CreateRawEvent("txn_002", "contract_002", DateTimeOffset.UtcNow.AddMinutes(-1));
        context.RawEvents.AddRange(olderRawEvent, newerRawEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new PaymentQueryService(context);

        var response = await service.GetPaymentsAsync(
            new() { Page = 1, PageSize = 1 },
            TestContext.Current.CancellationToken);

        response.Items.Should().ContainSingle();
        response.Items[0].TransactionId.Should().Be("txn_002");
        response.TotalItems.Should().Be(2);
        response.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetPaymentsAsyncShouldFilterByProcessingStatusAndContractId()
    {
        await using var context = CreateContext();
        context.RawEvents.AddRange(
            CreateRawEvent("txn_001", "contract_001", DateTimeOffset.UtcNow, ProcessingStatus.Processed),
            CreateRawEvent("txn_002", "contract_001", DateTimeOffset.UtcNow, ProcessingStatus.Failed),
            CreateRawEvent("txn_003", "contract_002", DateTimeOffset.UtcNow, ProcessingStatus.Failed));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new PaymentQueryService(context);

        var response = await service.GetPaymentsAsync(
            new()
            {
                Status = ProcessingStatus.Failed,
                ContractId = "contract_001"
            },
            TestContext.Current.CancellationToken);

        response.Items.Should().ContainSingle();
        response.Items[0].TransactionId.Should().Be("txn_002");
    }

    [Fact]
    public async Task GetPaymentAsyncShouldReturnRawPayloadAndContractStatus()
    {
        await using var context = CreateContext();
        var rawEvent = CreateRawEvent("txn_001", "contract_001", DateTimeOffset.UtcNow);
        context.RawEvents.Add(rawEvent);
        context.ContractStatuses.Add(new ContractStatus
        {
            ContractId = "contract_001",
            Status = "Paid",
            Amount = 10.50m,
            PaymentDate = DateTimeOffset.Parse("2026-09-01T00:00:00Z"),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new PaymentQueryService(context);

        var response = await service.GetPaymentAsync(rawEvent.Id, TestContext.Current.CancellationToken);

        response.Should().NotBeNull();
        response!.TransactionId.Should().Be("txn_001");
        response.PayloadJson.Should().Be(rawEvent.PayloadJson);
        response.PaymentStatus.Should().Be("Paid");
        response.Amount.Should().Be(10.50m);
    }

    [Fact]
    public async Task GetPaymentsAsyncShouldReturnRejectedRawEventsWithoutIdentifiers()
    {
        await using var context = CreateContext();
        var rawEvent = new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = null,
            ContractId = null,
            PayloadJson = "{",
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Failed,
            ProcessingError = "Invalid JSON payload.",
            IsProcessable = false
        };
        context.RawEvents.Add(rawEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new PaymentQueryService(context);

        var response = await service.GetPaymentsAsync(
            new() { Status = ProcessingStatus.Failed },
            TestContext.Current.CancellationToken);

        response.Items.Should().ContainSingle();
        response.Items[0].TransactionId.Should().BeNull();
        response.Items[0].ContractId.Should().BeNull();
        response.Items[0].ProcessingError.Should().Be("Invalid JSON payload.");
    }

    [Fact]
    public async Task GetPaymentAsyncShouldReturnRejectedRawEventWithoutIdentifiers()
    {
        await using var context = CreateContext();
        var rawEvent = new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = null,
            ContractId = null,
            PayloadJson = "{",
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Failed,
            ProcessingError = "Invalid JSON payload.",
            IsProcessable = false
        };
        context.RawEvents.Add(rawEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new PaymentQueryService(context);

        var response = await service.GetPaymentAsync(rawEvent.Id, TestContext.Current.CancellationToken);

        response.Should().NotBeNull();
        response!.TransactionId.Should().BeNull();
        response.ContractId.Should().BeNull();
        response.PayloadJson.Should().Be("{");
        response.ProcessingError.Should().Be("Invalid JSON payload.");
    }

    [Fact]
    public async Task GetPaymentAsyncShouldReturnNullWhenPaymentDoesNotExist()
    {
        await using var context = CreateContext();
        var service = new PaymentQueryService(context);

        var response = await service.GetPaymentAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        response.Should().BeNull();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RawEvent CreateRawEvent(
        string transactionId,
        string contractId,
        DateTimeOffset receivedAt,
        ProcessingStatus processingStatus = ProcessingStatus.Pending)
    {
        return new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            ContractId = contractId,
            PayloadJson = $$"""
                {"transaction_id":"{{transactionId}}","contract_id":"{{contractId}}","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
                """,
            ReceivedAt = receivedAt,
            ProcessingStatus = processingStatus
        };
    }
}
