using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payhook.Api.Data;
using Payhook.Api.Models;
using Payhook.Api.Tests.Infrastructure;
using Xunit;

namespace Payhook.Api.Tests.Controllers;

public sealed class PaymentsControllerTests
{
    [Fact]
    public async Task GetPaymentsShouldReturnPagedPayments()
    {
        await using var factory = new PayhookApiFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<TestPaginatedResponse<TestPaymentSummaryResponse>>(
            "/payments?page=1&pageSize=1",
            TestContext.Current.CancellationToken);

        response.Should().NotBeNull();
        response!.Items.Should().ContainSingle();
        response.TotalItems.Should().Be(2);
        response.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetPaymentsShouldFilterByStatusAndContractId()
    {
        await using var factory = new PayhookApiFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<TestPaginatedResponse<TestPaymentSummaryResponse>>(
            "/payments?status=Failed&contract_id=contract_001",
            TestContext.Current.CancellationToken);

        response.Should().NotBeNull();
        response!.Items.Should().ContainSingle();
        response.Items[0].TransactionId.Should().Be("txn_failed");
    }

    [Fact]
    public async Task GetPaymentShouldReturnPaymentDetail()
    {
        await using var factory = new PayhookApiFactory();
        var rawEventId = await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<TestPaymentDetailResponse>(
            $"/payments/{rawEventId}",
            TestContext.Current.CancellationToken);

        response.Should().NotBeNull();
        response!.TransactionId.Should().Be("txn_processed");
        response.PayloadJson.Should().Contain("txn_processed");
    }

    [Fact]
    public async Task GetPaymentShouldReturnNotFoundWhenPaymentDoesNotExist()
    {
        await using var factory = new PayhookApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/payments/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> SeedAsync(PayhookApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var processedRawEventId = Guid.NewGuid();

        context.RawEvents.AddRange(
            new RawEvent
            {
                Id = processedRawEventId,
                TransactionId = "txn_processed",
                ContractId = "contract_001",
                PayloadJson = "{\"transaction_id\":\"txn_processed\",\"contract_id\":\"contract_001\"}",
                ReceivedAt = DateTimeOffset.UtcNow.AddMinutes(-2),
                ProcessingStatus = ProcessingStatus.Processed
            },
            new RawEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = "txn_failed",
                ContractId = "contract_001",
                PayloadJson = "{\"transaction_id\":\"txn_failed\",\"contract_id\":\"contract_001\"}",
                ReceivedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                ProcessingStatus = ProcessingStatus.Failed,
                ProcessingError = "Invalid payment status."
            });
        context.ContractStatuses.Add(new ContractStatus
        {
            ContractId = "contract_001",
            Status = "Paid",
            Amount = 10.50m,
            PaymentDate = DateTimeOffset.Parse("2026-09-01T00:00:00Z"),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return processedRawEventId;
    }

    private sealed class TestPaginatedResponse<T>
    {
        public List<T> Items { get; init; } = [];

        public int TotalItems { get; init; }

        public int TotalPages { get; init; }
    }

    private sealed class TestPaymentSummaryResponse
    {
        public string TransactionId { get; init; } = string.Empty;
    }

    private sealed class TestPaymentDetailResponse
    {
        public string TransactionId { get; init; } = string.Empty;

        public string PayloadJson { get; init; } = string.Empty;
    }
}
