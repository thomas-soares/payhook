using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payhook.Api.Data;
using Payhook.Api.DTOs;
using Payhook.Api.Models;
using Payhook.Api.Options;

namespace Payhook.Api.Services;

public sealed class PaymentEventProcessor(
    ApplicationDbContext dbContext,
    IOptions<PaymentProcessingOptions> options,
    ILogger<PaymentEventProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan processingDelay = options.Value.ProcessingDelay;

    public async Task ProcessAsync(Guid rawEventId, CancellationToken cancellationToken)
    {
        var rawEvent = await dbContext.RawEvents
            .SingleOrDefaultAsync(candidate => candidate.Id == rawEventId, cancellationToken);

        if (rawEvent is null || !rawEvent.IsProcessable || rawEvent.ProcessingStatus != ProcessingStatus.Pending)
        {
            return;
        }

        try
        {
            if (processingDelay > TimeSpan.Zero)
            {
                await Task.Delay(processingDelay, cancellationToken);
            }

            var payment = JsonSerializer.Deserialize<PaymentWebhookRequest>(
                rawEvent.PayloadJson,
                JsonOptions);

            if (payment is null)
            {
                throw new JsonException("Payload could not be deserialized.");
            }

            await UpsertContractStatusAsync(payment, cancellationToken);

            rawEvent.ProcessingStatus = ProcessingStatus.Processed;
            rawEvent.ProcessingError = null;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to process raw event {RawEventId}.", rawEventId);

            rawEvent.ProcessingStatus = ProcessingStatus.Failed;
            rawEvent.ProcessingError = exception.Message;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> ProcessPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var rawEventIds = await dbContext.RawEvents
            .Where(rawEvent => rawEvent.ProcessingStatus == ProcessingStatus.Pending)
            .Where(rawEvent => rawEvent.IsProcessable)
            .OrderBy(rawEvent => rawEvent.ReceivedAt)
            .Take(batchSize)
            .Select(rawEvent => rawEvent.Id)
            .ToListAsync(cancellationToken);

        foreach (var rawEventId in rawEventIds)
        {
            await ProcessAsync(rawEventId, cancellationToken);
        }

        return rawEventIds.Count;
    }

    private async Task UpsertContractStatusAsync(
        PaymentWebhookRequest payment,
        CancellationToken cancellationToken)
    {
        var contractStatus = await dbContext.ContractStatuses
            .SingleOrDefaultAsync(
                candidate => candidate.ContractId == payment.ContractId,
                cancellationToken);

        if (contractStatus is null)
        {
            dbContext.ContractStatuses.Add(new ContractStatus
            {
                ContractId = payment.ContractId,
                Status = payment.Status,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate
                    ?? throw new InvalidOperationException("Payment date is required."),
                UpdatedAt = DateTimeOffset.UtcNow
            });

            return;
        }

        contractStatus.Status = payment.Status;
        contractStatus.Amount = payment.Amount;
        contractStatus.PaymentDate = payment.PaymentDate
            ?? throw new InvalidOperationException("Payment date is required.");
        contractStatus.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
