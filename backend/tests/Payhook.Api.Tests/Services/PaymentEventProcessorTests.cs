using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payhook.Api.Data;
using Payhook.Api.Models;
using Payhook.Api.Options;
using Payhook.Api.Services;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Payhook.Api.Tests.Services;

public sealed class PaymentEventProcessorTests
{
    [Fact]
    public async Task ProcessAsyncShouldUpdateContractStatusAndMarkRawEventAsProcessed()
    {
        await using var context = CreateContext();
        var rawEvent = new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = "txn_001",
            ContractId = "contract_001",
            PayloadJson = """
                {"transaction_id":"txn_001","contract_id":"contract_001","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
                """,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending
        };
        context.RawEvents.Add(rawEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var processor = CreateProcessor(context);

        await processor.ProcessAsync(rawEvent.Id, TestContext.Current.CancellationToken);

        rawEvent.ProcessingStatus.Should().Be(ProcessingStatus.Processed);
        rawEvent.ProcessingError.Should().BeNull();
        context.ContractStatuses.Should().ContainSingle(contractStatus =>
            contractStatus.ContractId == "contract_001"
            && contractStatus.Status == "Paid"
            && contractStatus.Amount == 10.50m);
    }

    [Fact]
    public async Task ProcessAsyncShouldMarkRawEventAsFailedWhenPayloadIsInvalid()
    {
        await using var context = CreateContext();
        var rawEvent = new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = "txn_001",
            ContractId = "contract_001",
            PayloadJson = "{",
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending
        };
        context.RawEvents.Add(rawEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var processor = CreateProcessor(context);

        await processor.ProcessAsync(rawEvent.Id, TestContext.Current.CancellationToken);

        rawEvent.ProcessingStatus.Should().Be(ProcessingStatus.Failed);
        rawEvent.ProcessingError.Should().NotBeNullOrWhiteSpace();
        context.ContractStatuses.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessPendingAsyncShouldProcessOldestPendingRawEventsUpToBatchSize()
    {
        await using var context = CreateContext();
        var olderRawEvent = CreateRawEvent(
            Guid.NewGuid(),
            "txn_001",
            "contract_001",
            DateTimeOffset.UtcNow.AddMinutes(-2));
        var newerRawEvent = CreateRawEvent(
            Guid.NewGuid(),
            "txn_002",
            "contract_002",
            DateTimeOffset.UtcNow.AddMinutes(-1));
        context.RawEvents.AddRange(olderRawEvent, newerRawEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var processor = CreateProcessor(context);

        var processedCount = await processor.ProcessPendingAsync(1, TestContext.Current.CancellationToken);

        processedCount.Should().Be(1);
        olderRawEvent.ProcessingStatus.Should().Be(ProcessingStatus.Processed);
        newerRawEvent.ProcessingStatus.Should().Be(ProcessingStatus.Pending);
        context.ContractStatuses.Should().ContainSingle(contractStatus =>
            contractStatus.ContractId == "contract_001");
    }

    private static PaymentEventProcessor CreateProcessor(ApplicationDbContext context)
    {
        var options = OptionsFactory.Create(new PaymentProcessingOptions
        {
            ProcessingDelay = TimeSpan.Zero
        });
        var logger = LoggerFactory.Create(static _ => { }).CreateLogger<PaymentEventProcessor>();

        return new PaymentEventProcessor(context, options, logger);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RawEvent CreateRawEvent(
        Guid id,
        string transactionId,
        string contractId,
        DateTimeOffset receivedAt)
    {
        return new RawEvent
        {
            Id = id,
            TransactionId = transactionId,
            ContractId = contractId,
            PayloadJson = $$"""
                {"transaction_id":"{{transactionId}}","contract_id":"{{contractId}}","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
                """,
            ReceivedAt = receivedAt,
            ProcessingStatus = ProcessingStatus.Pending
        };
    }
}
