using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payhook.Api.Data;
using Payhook.Api.Models;
using Xunit;

namespace Payhook.Api.Tests.Data;

public sealed class ApplicationDbContextModelTests
{
    [Fact]
    public void RawEventsShouldUseNullableTransactionIdAsFilteredUniqueIdempotencyKey()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(RawEvent));

        entityType.Should().NotBeNull();

        var transactionId = entityType!.FindProperty(nameof(RawEvent.TransactionId));
        var index = entityType.FindIndex([transactionId!]);

        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.GetFilter().Should().Be("transaction_id IS NOT NULL");
        index.GetDatabaseName().Should().Be("ix_raw_events_transaction_id");
        transactionId!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void RawEventsShouldStorePayloadAsText()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(RawEvent));

        entityType.Should().NotBeNull();

        var payload = entityType!.FindProperty(nameof(RawEvent.PayloadJson));

        payload.Should().NotBeNull();
        payload!.GetColumnType().Should().Be("text");
    }

    [Fact]
    public void RawEventsShouldAllowMissingContractId()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(RawEvent));

        entityType.Should().NotBeNull();

        var contractId = entityType!.FindProperty(nameof(RawEvent.ContractId));

        contractId.Should().NotBeNull();
        contractId!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void RawEventsShouldTrackWhetherTheyCanBeProcessed()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(RawEvent));

        entityType.Should().NotBeNull();

        var isProcessable = entityType!.FindProperty(nameof(RawEvent.IsProcessable));

        isProcessable.Should().NotBeNull();
        isProcessable!.GetDefaultValue().Should().Be(true);
    }

    [Fact]
    public void ContractStatusesShouldUseContractIdAsPrimaryKey()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ContractStatus));

        entityType.Should().NotBeNull();

        var primaryKey = entityType!.FindPrimaryKey();

        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle();
        primaryKey.Properties[0].Name.Should().Be(nameof(ContractStatus.ContractId));
        primaryKey.GetName().Should().Be("pk_contract_statuses");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=payhook;Username=payhook;Password=payhook")
            .Options;

        return new ApplicationDbContext(options);
    }
}
