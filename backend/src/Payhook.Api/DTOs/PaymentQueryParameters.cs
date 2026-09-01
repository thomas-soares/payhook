using Payhook.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Payhook.Api.DTOs;

public sealed class PaymentQueryParameters
{
    private const int MaxPageSize = 100;
    private int page = 1;
    private int pageSize = 20;

    public int Page
    {
        get => page;
        init => page = Math.Max(1, value);
    }

    public int PageSize
    {
        get => pageSize;
        init => pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    public ProcessingStatus? Status { get; init; }

    [FromQuery(Name = "contract_id")]
    public string? ContractId { get; init; }
}
