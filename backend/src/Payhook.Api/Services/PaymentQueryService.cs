using Microsoft.EntityFrameworkCore;
using Payhook.Api.Data;
using Payhook.Api.DTOs;

namespace Payhook.Api.Services;

public sealed class PaymentQueryService(ApplicationDbContext dbContext)
{
    public async Task<PaginatedResponse<PaymentSummaryResponse>> GetPaymentsAsync(
        PaymentQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var query = dbContext.RawEvents.AsNoTracking();

        if (parameters.Status is not null)
        {
            query = query.Where(rawEvent => rawEvent.ProcessingStatus == parameters.Status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ContractId))
        {
            query = query.Where(rawEvent => rawEvent.ContractId == parameters.ContractId);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(rawEvent => rawEvent.ReceivedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .GroupJoin(
                dbContext.ContractStatuses.AsNoTracking(),
                rawEvent => rawEvent.ContractId,
                contractStatus => contractStatus.ContractId,
                (rawEvent, contractStatuses) => new
                {
                    RawEvent = rawEvent,
                    ContractStatus = contractStatuses.FirstOrDefault()
                })
            .Select(payment => new PaymentSummaryResponse
            {
                Id = payment.RawEvent.Id,
                TransactionId = payment.RawEvent.TransactionId,
                ContractId = payment.RawEvent.ContractId,
                ProcessingStatus = payment.RawEvent.ProcessingStatus,
                ReceivedAt = payment.RawEvent.ReceivedAt,
                PaymentStatus = payment.ContractStatus == null ? null : payment.ContractStatus.Status,
                Amount = payment.ContractStatus == null ? null : payment.ContractStatus.Amount,
                PaymentDate = payment.ContractStatus == null ? null : payment.ContractStatus.PaymentDate,
                ProcessingError = payment.RawEvent.ProcessingError
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<PaymentSummaryResponse>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0
                ? 0
                : (int)Math.Ceiling(totalItems / (double)parameters.PageSize)
        };
    }

    public async Task<PaymentDetailResponse?> GetPaymentAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await dbContext.RawEvents
            .AsNoTracking()
            .Where(rawEvent => rawEvent.Id == id)
            .GroupJoin(
                dbContext.ContractStatuses.AsNoTracking(),
                rawEvent => rawEvent.ContractId,
                contractStatus => contractStatus.ContractId,
                (rawEvent, contractStatuses) => new
                {
                    RawEvent = rawEvent,
                    ContractStatus = contractStatuses.FirstOrDefault()
                })
            .Select(payment => new PaymentDetailResponse
            {
                Id = payment.RawEvent.Id,
                TransactionId = payment.RawEvent.TransactionId,
                ContractId = payment.RawEvent.ContractId,
                ProcessingStatus = payment.RawEvent.ProcessingStatus,
                ReceivedAt = payment.RawEvent.ReceivedAt,
                PayloadJson = payment.RawEvent.PayloadJson,
                PaymentStatus = payment.ContractStatus == null ? null : payment.ContractStatus.Status,
                Amount = payment.ContractStatus == null ? null : payment.ContractStatus.Amount,
                PaymentDate = payment.ContractStatus == null ? null : payment.ContractStatus.PaymentDate,
                UpdatedAt = payment.ContractStatus == null ? null : payment.ContractStatus.UpdatedAt,
                ProcessingError = payment.RawEvent.ProcessingError
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
