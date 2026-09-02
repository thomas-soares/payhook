using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Payhook.Api.DTOs;
using Payhook.Api.Services;

namespace Payhook.Api.Controllers;

[ApiController]
[Route("webhooks/payment")]
public sealed class PaymentWebhooksController(
    PaymentWebhookService webhookService,
    WebhookSecurityService securityService) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payloadJson = await reader.ReadToEndAsync(cancellationToken);
        var providedSignature = Request.Headers["X-Signature"].ToString();

        if (!securityService.IsValid(providedSignature, payloadJson))
        {
            await webhookService.StoreRejectedAsync(
                payloadJson,
                "Invalid signature.",
                transactionId: null,
                contractId: null,
                cancellationToken);

            return Unauthorized();
        }

        PaymentWebhookRequest? request;

        try
        {
            request = JsonSerializer.Deserialize<PaymentWebhookRequest>(payloadJson, JsonOptions);
        }
        catch (JsonException)
        {
            await webhookService.StoreRejectedAsync(
                payloadJson,
                "Invalid JSON payload.",
                transactionId: null,
                contractId: null,
                cancellationToken);

            return BadRequest();
        }

        if (request is null)
        {
            await webhookService.StoreRejectedAsync(
                payloadJson,
                "Empty JSON payload.",
                transactionId: null,
                contractId: null,
                cancellationToken);

            return BadRequest();
        }

        if (!TryValidateRequest(request, out var validationResults))
        {
            await webhookService.StoreRejectedAsync(
                payloadJson,
                BuildValidationErrorMessage(validationResults),
                request.TransactionId,
                request.ContractId,
                cancellationToken);

            return BadRequest(new ValidationProblemDetails(ToValidationErrors(validationResults)));
        }

        var result = await webhookService.ReceiveAsync(
            request,
            payloadJson,
            cancellationToken);

        return result is PaymentWebhookResult.Duplicate
            ? Ok()
            : Accepted();
    }

    private static bool TryValidateRequest(
        PaymentWebhookRequest request,
        out List<ValidationResult> validationResults)
    {
        validationResults = [];
        var validationContext = new ValidationContext(request);

        return Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true);
    }

    private static Dictionary<string, string[]> ToValidationErrors(
        IEnumerable<ValidationResult> validationResults)
    {
        return validationResults
            .SelectMany(validationResult =>
            {
                var memberNames = validationResult.MemberNames.Any()
                    ? validationResult.MemberNames
                    : [string.Empty];

                return memberNames.Select(memberName => new
                {
                    MemberName = memberName,
                    ErrorMessage = validationResult.ErrorMessage ?? "The request is invalid."
                });
            })
            .GroupBy(error => error.MemberName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }

    private static string BuildValidationErrorMessage(IEnumerable<ValidationResult> validationResults)
    {
        return string.Join(
            " ",
            validationResults.Select(validationResult =>
                validationResult.ErrorMessage ?? "The request is invalid."));
    }
}
