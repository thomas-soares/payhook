using Microsoft.AspNetCore.Mvc;
using Payhook.Api.DTOs;
using Payhook.Api.Services;

namespace Payhook.Api.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController(PaymentQueryService paymentQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<PaymentSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<PaymentSummaryResponse>>> GetPayments(
        [FromQuery] PaymentQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var response = await paymentQueryService.GetPaymentsAsync(parameters, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDetailResponse>> GetPayment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await paymentQueryService.GetPaymentAsync(id, cancellationToken);

        return response is null
            ? NotFound()
            : Ok(response);
    }
}
