using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payhook.Api.Data;
using Payhook.Api.DTOs;
using Payhook.Api.Models;

namespace Payhook.Api.Services;

public sealed class PaymentWebhookService(
    ApplicationDbContext dbContext,
    IPaymentProcessingQueue processingQueue)
{
    private const string UniqueViolationSqlState = "23505";

    public async Task<PaymentWebhookResult> ReceiveAsync(
        PaymentWebhookRequest request,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var alreadyReceived = await dbContext.RawEvents
            .AnyAsync(rawEvent => rawEvent.TransactionId == request.TransactionId, cancellationToken);

        if (alreadyReceived)
        {
            return PaymentWebhookResult.Duplicate;
        }

        var rawEvent = new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = request.TransactionId,
            ContractId = request.ContractId,
            PayloadJson = payloadJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending,
            IsProcessable = true
        };

        dbContext.RawEvents.Add(rawEvent);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return PaymentWebhookResult.Duplicate;
        }

        await processingQueue.EnqueueAsync(rawEvent.Id, cancellationToken);

        return PaymentWebhookResult.Accepted;
    }

    public async Task StoreRejectedAsync(
        string payloadJson,
        string processingError,
        string? transactionId,
        string? contractId,
        CancellationToken cancellationToken)
    {
        var rawEvent = new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = string.IsNullOrWhiteSpace(transactionId) ? null : transactionId,
            ContractId = string.IsNullOrWhiteSpace(contractId) ? null : contractId,
            PayloadJson = payloadJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Failed,
            ProcessingError = processingError,
            IsProcessable = false
        };

        dbContext.RawEvents.Add(rawEvent);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(rawEvent).State = EntityState.Detached;
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == UniqueViolationSqlState;
    }
}
