using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payhook.Api.Data;
using Payhook.Api.DTOs;
using Payhook.Api.Models;

namespace Payhook.Api.Services;

public sealed class PaymentWebhookService(ApplicationDbContext dbContext)
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

        dbContext.RawEvents.Add(new RawEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = request.TransactionId,
            PayloadJson = payloadJson,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return PaymentWebhookResult.Duplicate;
        }

        return PaymentWebhookResult.Accepted;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == UniqueViolationSqlState;
    }
}
